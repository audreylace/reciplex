using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Reciplex.Server.Database.SearchExtractionWorker;

/// <summary>
/// Worker service for extracting records to an external search index
/// </summary>
/// <param name="options">search extraction options</param>
internal sealed class SearchExtractionWorkerService(
    IOptions<SearchExtractionOptions> options,
    IClock clock,
    IServiceProvider sp
) : BackgroundService, IDurableChangeQueueEntryPostedBroadcaster
{
    /// <summary>
    /// Max entries in <see cref="_entries"/> and <see cref="_entriesInQueue"/>
    /// </summary>
    private const int MaxEntries = 1_000;

    /// <summary>
    /// Set of entries to work through
    /// </summary>
    private readonly Queue<long> _entries = new();

    /// <summary>
    /// Set of entries in <see cref="_entries" /> but indexed by id for deduplication
    /// </summary>
    private readonly HashSet<long> _entriesInQueue = [];

    /// <summary>
    /// Channel buffering incoming messages via <see cref="IDurableChangeQueueEntryPostedBroadcaster"/>
    /// </summary>
    private readonly Channel<long> _channel = Channel.CreateBounded<long>(128);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enable)
        {
            return;
        }

        Instant last = Instant.MinValue;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // wait for next entry for up to 60 seconds
                CancellationTokenSource channelCancel =
                    CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                channelCancel.CancelAfter(TimeSpan.FromSeconds(60));
                try
                {
                    long? nextId = await GetNextChannelEntryAsync(channelCancel.Token);
                }
                catch (OperationCanceledException)
                    when (channelCancel.IsCancellationRequested
                        && !stoppingToken.IsCancellationRequested
                    ) { }
                // dbChannel ??= GetNextEntryFromDatabase(stoppingToken);

                // await Task.WhenAny(queueChannel, dbChannel);

                // if(queueChannel)

                if (clock.GetCurrentInstant() - last > Duration.FromSeconds(60))
                {
                    await FindStaleEntriesAsync(stoppingToken);
                    last = clock.GetCurrentInstant();
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // todo - log
            }
        }

        return;
    }

    public void BroadcastEntryAdded(long id)
    {
        _channel.Writer.TryWrite(id);
    }

    private async Task<long?> GetNextChannelEntryAsync(CancellationToken ct)
    {
        try
        {
            while (await _channel.Reader.WaitToReadAsync(ct) && !ct.IsCancellationRequested)
            {
                if (_channel.Reader.TryRead(out long item))
                {
                    return item;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // todo - log
        }

        return null;
    }
}
