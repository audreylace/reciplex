using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.SearchExporter.Loggers;
using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter;

/// <summary>
/// renews lease rows
/// </summary>
/// <param name="recipeSearchExportStatusRepository">repository storing search status rows</param>
/// <param name="logger">logger for this class</param>
class LeaseRenewer(
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
    /// <param name="frequencyMs">the cadence of lease renewal</param>
    /// <param name="maxRetries">max number of tries</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>task that resolves on cancellation or when there are no more leases</returns>
    public async Task ExecuteAsync(
        string leaseToken,
        List<long> ids,
        long leaseExpireWindow,
        int frequencyMs,
        int maxRetries,
        CancellationToken ct
    )
    {
        int retry = 0;
        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (
                    await recipeSearchExportStatusRepository.RenewLeasesAsync(
                        ids,
                        leaseToken,
                        leaseExpireWindow,
                        ct
                    ) < 1
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

            try
            {
                await Task.Delay(frequencyMs, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.Error_ExceptionDuringPause(ex);
            }
        }
    }
}
