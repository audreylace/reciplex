using Reciplex.Server.Database.SearchExporter.Repositories;

namespace Reciplex.Server.Database.SearchExporter;

class LeaseRenewer(
    IRecipeSearchExportStatusRepository recipeSearchExportStatusRepository,
    string leaseToken,
    List<long> ids,
    long leaseExpireWindow,
    int frequencyMs
)
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        int maxRetries = 10;
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
                    break;
                }
                retry = 0;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception)
            {
                retry++;
                if (retry > maxRetries)
                {
                    throw;
                }
            }

            try
            {
                await Task.Delay(frequencyMs, ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
