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
internal class DeletionWorkerService(
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
            backoff = Math.Min(10, backoff + 1);
            metrics.SetRunningStatus(true);
            bool anyWorkDone = false;
            anyWorkDone |= await RunUntilCompletionWithDelay(
                CleanupDeletedRecipesAsync,
                DeletionWorkerServiceMetrics.RecipesVariant,
                stoppingToken
            );
            anyWorkDone |= await RunUntilCompletionWithDelay(
                CleanupRecipesBookAdditionalUsersAsync,
                DeletionWorkerServiceMetrics.RecipeBookAccessEntries,
                stoppingToken
            );
            anyWorkDone |= await RunUntilCompletionWithDelay(
                CleanupDeletedRecipesBooksAsync,
                DeletionWorkerServiceMetrics.RecipeBooksVariant,
                stoppingToken
            );
            anyWorkDone |= await RunUntilCompletionWithDelay(
                CleanupDeletedUsersAsync,
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
            metrics.ObserveSleepTime(minutes);
            await Task.Delay(TimeSpan.FromMinutes(minutes), stoppingToken);
        }
    }

    private async Task<bool> CleanupDeletedRecipesAsync(
        ApplicationDbContext db,
        string databaseObjectName,
        CancellationToken ct
    )
    {
        long startTimestamp = Stopwatch.GetTimestamp();
        var recipesToDeleteById = await db
            .Recipes.Where(r =>
                r.Deleted != null
                || r.RecipeBook!.Deleted != null
                || r.RecipeBook!.Owner!.Deleted != null
            )
            .Select(r => r.Id)
            .Take(100)
            .ToListAsync(ct);
        metrics.ObserveDeletionCandidateQuery(
            databaseObjectName,
            Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds
        );

        if (recipesToDeleteById.Count <= 0)
        {
            return false; // we did nothing!
        }

        startTimestamp = Stopwatch.GetTimestamp();
        long rows = await db
            .Recipes.Where(r => recipesToDeleteById.Contains(r.Id))
            .ExecuteDeleteAsync(ct);
        metrics.ObserveDeletionCandidateQuery(
            databaseObjectName,
            Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds
        );

        metrics.IncRowsDeleted(rows, databaseObjectName);
        return rows > 0;
    }

    private async Task<bool> CleanupRecipesBookAdditionalUsersAsync(
        ApplicationDbContext db,
        string databaseObjectName,
        CancellationToken ct
    )
    {
        long startTimestamp = Stopwatch.GetTimestamp();
        var accessEntryById = await db
            .RecipeBookAccessEntries.Where(rAccessEntry =>
                (
                    rAccessEntry.User!.Deleted != null
                    || rAccessEntry.RecipeBook!.Deleted != null
                    || rAccessEntry.RecipeBook!.Owner!.Deleted != null
                )
            )
            .Select(r => r.Id)
            .Take(100)
            .ToListAsync(ct);
        metrics.ObserveDeletionCandidateQuery(
            databaseObjectName,
            Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds
        );

        if (accessEntryById.Count <= 0)
        {
            return false; // we did nothing!
        }

        startTimestamp = Stopwatch.GetTimestamp();
        long rows = await db
            .RecipeBookAccessEntries.Where(r => accessEntryById.Contains(r.Id))
            .ExecuteDeleteAsync(ct);
        metrics.ObserveDeletionQuery(
            databaseObjectName,
            Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds
        );
        metrics.IncRowsDeleted(rows, databaseObjectName);

        return rows > 0;
    }

    private async Task<bool> CleanupDeletedRecipesBooksAsync(
        ApplicationDbContext db,
        string databaseObjectName,
        CancellationToken ct
    )
    {
        long startTimestamp = Stopwatch.GetTimestamp();
        var recipeBooksToDeleteById = await db
            .RecipeBooks.Where(r =>
                (r.Deleted != null || r.Owner!.Deleted != null)
                && !r.Recipes.Any()
                && !r.AdditionalUsers.Any()
            )
            .Select(r => r.Id)
            .Take(100)
            .ToListAsync(ct);
        metrics.ObserveDeletionCandidateQuery(
            databaseObjectName,
            Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds
        );

        if (recipeBooksToDeleteById.Count <= 0)
        {
            return false; // we did nothing!
        }

        startTimestamp = Stopwatch.GetTimestamp();
        long rows = await db
            .RecipeBooks.Where(r => recipeBooksToDeleteById.Contains(r.Id))
            .ExecuteDeleteAsync(ct);
        metrics.ObserveDeletionQuery(
            databaseObjectName,
            Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds
        );
        metrics.IncRowsDeleted(rows, databaseObjectName);

        return rows > 0;
    }

    private async Task<bool> CleanupDeletedUsersAsync(
        ApplicationDbContext db,
        string databaseObjectName,
        CancellationToken ct
    )
    {
        long startTimestamp = Stopwatch.GetTimestamp();
        var usersToDeleteById = await db
            .Users.Where(user =>
                user.Deleted != null
                && !user.RecipeBookAccessEntities.Any()
                && !user.BooksTheUserOwns.Any()
            )
            .Select(r => r.Id)
            .Take(100)
            .ToListAsync(ct);
        metrics.ObserveDeletionCandidateQuery(
            databaseObjectName,
            Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds
        );

        if (usersToDeleteById.Count <= 0)
        {
            return false; // we did nothing!
        }

        startTimestamp = Stopwatch.GetTimestamp();
        long count = await db
            .Users.Where(r => usersToDeleteById.Contains(r.Id))
            .ExecuteDeleteAsync(ct);
        metrics.ObserveDeletionQuery(
            databaseObjectName,
            Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds
        );

        metrics.IncRowsDeleted(count, databaseObjectName);
        return count > 0;
    }

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
