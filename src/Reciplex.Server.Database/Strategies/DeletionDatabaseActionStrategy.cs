using System.Diagnostics;

namespace Reciplex.Server.Database.Strategies;

internal static class DeletionDatabaseActionStrategy
{
    /// <summary>
    /// Collects ids to delete and then deletes them
    /// </summary>
    /// <param name="collect">the query to get the ids list</param>
    /// <param name="delete">the query to execute the delete on the id list</param>
    /// <param name="observeOperation">Metric observer</param>
    /// <returns>delegate to hand off to
    /// <see cref="RepeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(Func{ApplicationDbContext, CancellationToken, Task{bool}}, Action{bool, double}, Action{Exception}, CancellationToken)"/>
    /// </returns>
    internal static Func<ApplicationDbContext, CancellationToken, Task<bool>> CollectAndDelete(
        Func<ApplicationDbContext, CancellationToken, Task<List<long>>> collect,
        Func<ApplicationDbContext, List<long>, CancellationToken, Task<int>> delete,
        Action<double, double, long> observeOperation
    )
    {
        return async (db, ct) =>
        {
            long startTimestamp = Stopwatch.GetTimestamp();
            var ids = await collect(db, ct);
            double collectQueryTime = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

            if (ids.Count <= 0)
            {
                return false; // we did nothing!
            }

            startTimestamp = Stopwatch.GetTimestamp();
            long count = await delete(db, ids, ct);

            observeOperation(
                collectQueryTime,
                Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds,
                count
            );

            return count > 0;
        };
    }
}
