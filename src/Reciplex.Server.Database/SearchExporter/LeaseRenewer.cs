using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter;

/// <summary>
/// renews lease rows
/// </summary>
/// <param name="recipeSearchExportStatusRepository">repository storing search status rows</param>
/// <param name="logger">logger for this class</param>
sealed class LeaseRenewer(
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository,
    ILogger<LeaseRenewer> logger
)
{
    /// <summary>
    /// Runs a lease renewal loop
    /// </summary>
    /// <param name="leaseToken">lease token</param>
    /// <param name="ids">set of ids to renew</param>
    /// <param name="leaseExpireWindow">the window after which the lease expires</param>
    /// <param name="frequency">the cadence of lease renewal</param>
    /// <param name="maxRetries">max number of tries</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>task that resolves on cancellation or when there are no more leases</returns>
    public async Task ExecuteAsync(
        string leaseToken,
        List<long> ids,
        TimeSpan leaseExpireWindow,
        TimeSpan frequency,
        int maxRetries,
        CancellationToken ct
    )
    {
        int retry = 0;
        using PeriodicTimer periodicTimer = new(frequency);
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await periodicTimer.WaitForNextTickAsync(ct);
                if (
                    await recipeSearchExportStatusRepository.RenewLeasesAsync(
                        ids,
                        leaseToken,
                        leaseExpireWindow,
                        ct
                    ) != ids.Count
                )
                {
                    return;
                }
                retry = 0;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                retry++;
                if (retry > maxRetries)
                {
                    logger.Error_MaxLeaseRenewAttempt(retry, ex);
                    return;
                }
                else
                {
                    logger.Warning_LeaseRenewAttemptFailed(retry, ex);
                }
            }
        }
    }
}
