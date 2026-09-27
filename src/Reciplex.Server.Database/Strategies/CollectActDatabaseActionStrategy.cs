using Microsoft.Extensions.DependencyInjection;

namespace Reciplex.Server.Database.Strategies;

internal static class CollectActDatabaseActionStrategy
{
    /// <summary>
    /// Collects a set of items and then operates on them
    /// </summary>
    /// <param name="collect">the query to get the list</param>
    /// <param name="act">the query to execute the act on the list</param>
    /// <param name="observeOperation">Metric observer</param>
    /// <returns>delegate to hand off to
    /// <see cref="RepeatedDatabaseActionStrategy.RunUntilCompletionWithDelay(Func{ApplicationDbContext, CancellationToken, Task{bool}}, Action{bool, double}, Action{Exception}, CancellationToken)"/>
    /// </returns>
    internal static Func<ApplicationDbContext, CancellationToken, Task<bool>> CollectAndAct<T>(
        Func<ApplicationDbContext, CancellationToken, Task<List<T>>> collect,
        Func<ApplicationDbContext, List<T>, CancellationToken, Task<int>> act,
        Action<long> observeOperation
    )
    {
        return async (db, ct) =>
        {
            var ids = await collect(db, ct);
            if (ids.Count <= 0)
            {
                return false; // we did nothing!
            }

            long count = await act(db, ids, ct);
            observeOperation(count);

            return count > 0;
        };
    }

    internal static Func<
        ApplicationDbContext,
        AsyncServiceScope,
        CancellationToken,
        Task<bool>
    > CollectAndAct<T>(
        Func<ApplicationDbContext, AsyncServiceScope, CancellationToken, Task<List<T>>> collect,
        Func<ApplicationDbContext, AsyncServiceScope, List<T>, CancellationToken, Task<int>> act,
        Action<long> observeOperation
    )
    {
        return async (db, scope, ct) =>
        {
            var ids = await collect(db, scope, ct);
            if (ids.Count <= 0)
            {
                return false; // we did nothing!
            }

            long count = await act(db, scope, ids, ct);

            observeOperation(count);

            return count > 0;
        };
    }
}
