using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.Strategies;

namespace Reciplex.Server.Database.DeletionWorker;

/// <summary>
/// Background worker that finishes deletion of documents
/// </summary>
/// <param name="sp">Service provider for opening scopes.</param>
/// <param name="metrics">metrics for DeletionWorkerService</param>
/// <param name="logger">Logger for writing exceptions and diagnostics</param>
internal sealed class DeletionWorkerService(
    RepeatedDatabaseActionStrategy repeatedDatabaseActionStrategy,
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
                DeletionDatabaseActionStrategy.CollectAndDelete(
                    (db, ct) =>
                        db
                            .Recipes.Where(r =>
                                (
                                    r.Deleted != null
                                    || r.RecipeBook!.Deleted != null
                                    || r.RecipeBook!.Owner!.Deleted != null
                                )
                                && r.RecipeSearchExtraction == null
                            )
                            .Select(r => r.Id)
                            .Take(100)
                            .ToListAsync(ct),
                    (db, ids, ct) =>
                        db.Recipes.Where(r => ids.Contains(r.Id)).ExecuteDeleteAsync(ct),
                    MetricObserver(DeletionWorkerServiceMetrics.RecipesVariant)
                ),
                DeletionWorkerServiceMetrics.RecipesVariant,
                stoppingToken
            );

            anyWorkDone |= await RunUntilCompletionWithDelay(
                DeletionDatabaseActionStrategy.CollectAndDelete(
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
                            .ExecuteDeleteAsync(ct),
                    MetricObserver(DeletionWorkerServiceMetrics.RecipeBookAccessEntries)
                ),
                DeletionWorkerServiceMetrics.RecipeBookAccessEntries,
                stoppingToken
            );

            anyWorkDone |= await RunUntilCompletionWithDelay(
                DeletionDatabaseActionStrategy.CollectAndDelete(
                    (db, ct) =>
                        db
                            .RecipeBooks.Where(r =>
                                (r.Deleted != null || r.Owner!.Deleted != null)
                                && !r.Recipes.Any()
                                && !r.AdditionalUsers.Any()
                            )
                            .Select(r => r.Id)
                            .Take(100)
                            .ToListAsync(ct),
                    (db, ids, ct) =>
                        db.RecipeBooks.Where(r => ids.Contains(r.Id)).ExecuteDeleteAsync(ct),
                    MetricObserver(DeletionWorkerServiceMetrics.RecipeBooksVariant)
                ),
                DeletionWorkerServiceMetrics.RecipeBooksVariant,
                stoppingToken
            );

            anyWorkDone |= await RunUntilCompletionWithDelay(
                DeletionDatabaseActionStrategy.CollectAndDelete(
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
                    (db, ids, ct) => db.Users.Where(r => ids.Contains(r.Id)).ExecuteDeleteAsync(ct),
                    MetricObserver(DeletionWorkerServiceMetrics.UsersVariant)
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

    private Action<double, double, long> MetricObserver(string databaseObjectName)
    {
        return (collectTime, time, rows) =>
        {
            metrics.ObserveDeletionCandidateQuery(databaseObjectName, collectTime);
            metrics.ObserveDeletionQuery(databaseObjectName, time);
            metrics.IncRowsDeleted(rows, databaseObjectName);
        };
    }

    /// <summary>
    /// Runs a database operation over and over with delay until <paramref name="action"/> returns false
    /// </summary>
    /// <param name="action">the database action to run</param>
    /// <param name="databaseObjectName">name of the operation for metrics</param>
    /// <param name="ct">async cancellation token</param>
    /// <returns>true if anything any real work was completed</returns>
    internal async Task<bool> RunUntilCompletionWithDelay(
        Func<ApplicationDbContext, CancellationToken, Task<bool>> action,
        string databaseObjectName,
        CancellationToken ct
    )
    {
        return await repeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(
            (db, ct) => action(db, ct),
            (result, time) => metrics.RecordOperationOutcome(result, databaseObjectName, time),
            ex => logger.RunUntilCompletionWithDelayError(databaseObjectName, ex),
            ct
        );
    }
}
