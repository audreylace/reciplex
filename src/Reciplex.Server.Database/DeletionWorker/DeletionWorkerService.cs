using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.Strategies;

namespace Reciplex.Server.Database.DeletionWorker;

/// <summary>
/// Background worker that finishes deletion of documents
/// </summary>
/// <param name="repeatedDatabaseActionStrategy">Strategy for running a collect followed by act operation</param>
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
        using PeriodicTimer periodicTimer = new(TimeSpan.FromMinutes(1));
        while (
            !stoppingToken.IsCancellationRequested
            && await periodicTimer.WaitForNextTickAsync(stoppingToken)
        )
        {
            bool success = true;
            long startTimestamp = Stopwatch.GetTimestamp();
            try
            {
                await RunUntilCompletionWithDelay(
                    CollectActDatabaseActionStrategy.CollectAndAct(
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
                                .OrderBy(r => r.Id)
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

                await RunUntilCompletionWithDelay(
                    CollectActDatabaseActionStrategy.CollectAndAct(
                        (db, ct) =>
                            db
                                .RecipeBookAccessEntries.Where(rAccessEntry =>
                                    (
                                        rAccessEntry.User!.Deleted != null
                                        || rAccessEntry.RecipeBook!.Deleted != null
                                        || rAccessEntry.RecipeBook!.Owner!.Deleted != null
                                    )
                                )
                                .OrderBy(r => r.Id)
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

                await RunUntilCompletionWithDelay(
                    CollectActDatabaseActionStrategy.CollectAndAct(
                        (db, ct) =>
                            db
                                .RecipeBooks.Where(r =>
                                    (r.Deleted != null || r.Owner!.Deleted != null)
                                    && !r.Recipes.Any()
                                    && !r.AdditionalUsers.Any()
                                )
                                .OrderBy(r => r.Id)
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

                await RunUntilCompletionWithDelay(
                    CollectActDatabaseActionStrategy.CollectAndAct(
                        (db, ct) =>
                            db
                                .Users.Where(user =>
                                    user.Deleted != null
                                    && !user.RecipeBookAccessEntities.Any()
                                    && !user.BooksTheUserOwns.Any()
                                )
                                .OrderBy(r => r.Id)
                                .Select(r => r.Id)
                                .Take(100)
                                .ToListAsync(ct),
                        (db, ids, ct) =>
                            db.Users.Where(r => ids.Contains(r.Id)).ExecuteDeleteAsync(ct),
                        MetricObserver(DeletionWorkerServiceMetrics.UsersVariant)
                    ),
                    DeletionWorkerServiceMetrics.UsersVariant,
                    stoppingToken
                );
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.Error_ExceptionInMainLoop(ex);
                success = false;
            }
            metrics.ObserveMainLoop(Stopwatch.GetElapsedTime(startTimestamp), success);
        }
    }

    private Action<long> MetricObserver(string databaseObjectName)
    {
        return (rows) =>
        {
            metrics.ObserveRowsDeleted(rows, databaseObjectName);
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
            (result) => metrics.ObserveOperationOutcome(databaseObjectName, result),
            ex => logger.RunUntilCompletionWithDelayError(databaseObjectName, ex),
            ct
        );
    }
}
