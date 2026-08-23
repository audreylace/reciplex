using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Reciplex.Server.Database.DeletionWorker;

/// <summary>
/// Background worker that finishes deletion of documents
/// </summary>
/// <param name="sp">Service provider for opening scopes.</param>
/// <param name="metrics">metrics for DeletionWorkerService</param>
/// <param name="logger">Logger for writing exceptions and diagnostics</param>
internal sealed class DeletionWorkerService(
    IServiceProvider sp,
    DeletionWorkerServiceMetrics metrics,
    ILogger<DeletionWorkerService> logger
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int backoff = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            long startTimestamp = Stopwatch.GetTimestamp();
            backoff = Math.Min(10, backoff + 1);
            metrics.SetRunningStatus(true);
            bool anyWorkDone = false;

            anyWorkDone |= await RunUntilCompletionWithDelay(
                CollectAndDelete(
                    (db, ct) =>
                        db
                            .Recipes.Where(r =>
                                (
                                    r.Deleted != null
                                    || r.RecipeBook!.Deleted != null
                                    || r.RecipeBook!.Owner!.Deleted != null
                                )
                                && !r.ChangeQueueEntries.Any()
                                && r.ExternalSearchIndexEntry == null
                            )
                            .Select(r => r.Id)
                            .Take(100)
                            .ToListAsync(ct),
                    (db, ids, ct) =>
                        db.Recipes.Where(r => ids.Contains(r.Id)).ExecuteDeleteAsync(ct)
                ),
                DeletionWorkerServiceMetrics.RecipesVariant,
                stoppingToken
            );

            anyWorkDone |= await RunUntilCompletionWithDelay(
                CollectAndDelete(
                    (db, ct) =>
                        db
                            .RecipeBookAccessEntries.Where(rAccessEntry =>
                                (
                                    rAccessEntry.User!.Deleted != null
                                    || rAccessEntry.RecipeBook!.Deleted != null
                                    || rAccessEntry.RecipeBook!.Owner!.Deleted != null
                                )
                            )
                            .Select(r => r.Id)
                            .Take(100)
                            .ToListAsync(ct),
                    (db, ids, ct) =>
                        db
                            .RecipeBookAccessEntries.Where(r => ids.Contains(r.Id))
                            .ExecuteDeleteAsync(ct)
                ),
                DeletionWorkerServiceMetrics.RecipeBookAccessEntries,
                stoppingToken
            );

            anyWorkDone |= await RunUntilCompletionWithDelay(
                CollectAndDelete(
                    (db, ct) =>
                        db
                            .RecipeBooks.Where(r =>
                                (r.Deleted != null || r.Owner!.Deleted != null)
                                && !r.Recipes.Any()
                                && !r.AdditionalUsers.Any()
                                && !r.ChangeQueueEntries.Any()
                                && r.ExternalSearchIndexEntry == null
                            )
                            .Select(r => r.Id)
                            .Take(100)
                            .ToListAsync(ct),
                    (db, ids, ct) =>
                        db.RecipeBooks.Where(r => ids.Contains(r.Id)).ExecuteDeleteAsync(ct)
                ),
                DeletionWorkerServiceMetrics.RecipeBooksVariant,
                stoppingToken
            );

            anyWorkDone |= await RunUntilCompletionWithDelay(
                CollectAndDelete(
                    (db, ct) =>
                        db
                            .Users.Where(user =>
                                user.Deleted != null
                                && !user.RecipeBookAccessEntities.Any()
                                && !user.BooksTheUserOwns.Any()
                            )
                            .Select(r => r.Id)
                            .Take(100)
                            .ToListAsync(ct),
                    (db, ids, ct) => db.Users.Where(r => ids.Contains(r.Id)).ExecuteDeleteAsync(ct)
                ),
                DeletionWorkerServiceMetrics.UsersVariant,
                stoppingToken
            );

            if (anyWorkDone)
            {
                backoff = 1;
            }

            double minutes = Math.Min(60, Math.Pow(2, backoff - 1));

            if (anyWorkDone)
            {
                logger.CleanupWorkDone(minutes);
            }
            else
            {
                logger.NoCleanupWorkDone(minutes);
            }

            metrics.SetRunningStatus(false);
            metrics.ObserveMainLoop(
                minutes,
                Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds
            );
            await Task.Delay(TimeSpan.FromMinutes(minutes), stoppingToken);
        }
    }

    /// <summary>
    /// Collects ids to delete and then deletes them
    /// </summary>
    /// <param name="collect">the query to get the ids list</param>
    /// <param name="delete">the query to execute the delete on the id list</param>
    /// <returns>delegate to hand off to <see cref="RunUntilCompletionWithDelay"/> </returns>
    private Func<ApplicationDbContext, string, CancellationToken, Task<bool>> CollectAndDelete(
        Func<ApplicationDbContext, CancellationToken, Task<List<long>>> collect,
        Func<ApplicationDbContext, List<long>, CancellationToken, Task<int>> delete
    )
    {
        return async (db, databaseObjectName, ct) =>
        {
            long startTimestamp = Stopwatch.GetTimestamp();
            var ids = await collect(db, ct);
            metrics.ObserveDeletionCandidateQuery(
                databaseObjectName,
                Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds
            );

            if (ids.Count <= 0)
            {
                return false; // we did nothing!
            }

            startTimestamp = Stopwatch.GetTimestamp();
            long count = await delete(db, ids, ct);
            metrics.ObserveDeletionQuery(
                databaseObjectName,
                Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds
            );

            metrics.IncRowsDeleted(count, databaseObjectName);
            return count > 0;
        };
    }

    /// <summary>
    /// Runs a database operation over and over with delay until <paramref name="action"/> returns false
    /// </summary>
    /// <param name="action">the database action to run</param>
    /// <param name="databaseObjectName">name of the operation for metrics</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>true if anything any real work was completed</returns>
    private async Task<bool> RunUntilCompletionWithDelay(
        Func<ApplicationDbContext, string, CancellationToken, Task<bool>> action,
        string databaseObjectName,
        CancellationToken ct
    )
    {
        bool anyWorkDone = false;
        bool workDone;
        do
        {
            long startTimestamp = Stopwatch.GetTimestamp();
            try
            {
                await using AsyncServiceScope scope = sp.CreateAsyncScope();
                await using ApplicationDbContext db =
                    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                workDone = await action(db, databaseObjectName, ct);
                anyWorkDone |= workDone;
                metrics.RecordOperationOutcome(
                    true,
                    databaseObjectName,
                    Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds
                );
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.RunUntilCompletionWithDelayError(databaseObjectName, ex);
                metrics.RecordOperationOutcome(
                    false,
                    databaseObjectName,
                    Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds
                );
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
                break;
            }
            await Task.Delay(TimeSpan.FromSeconds(1), ct);
        } while (workDone);

        return anyWorkDone;
    }
}
