using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Reciplex.Server.Database.Strategies;

/// <summary>
/// Strategies to run actions over and over against the database
/// </summary>
/// <param name="sp">service provider to open up database scopes</param>
internal class RepeatedDatabaseActionStrategy(IServiceProvider sp)
{
    /// <summary>
    /// Runs a database operation over and over with delay until <paramref name="action"/> returns false
    /// </summary>
    /// <param name="action">the database action to run</param>
    /// <param name="recordMetrics">metric recorder</param>
    /// <param name="exceptionLogger">Logger for exceptions</params>
    /// <param name="ct">async cancellation token</param>
    /// <returns>true if anything any real work was completed</returns>
    internal async Task<bool> RunUntilCompletionWithDelay(
        Func<ApplicationDbContext, CancellationToken, Task<bool>> action,
        Action<bool, double> recordMetrics,
        Action<Exception> exceptionLogger,
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

                workDone = await action(db, ct);
                anyWorkDone |= workDone;
                recordMetrics(true, Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                exceptionLogger(ex);
                recordMetrics(false, Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
                break;
            }
            await Task.Delay(TimeSpan.FromSeconds(1), ct);
        } while (workDone);

        return anyWorkDone;
    }

    /// <summary>
    /// Runs a database operation over and over with delay until <paramref name="action"/> returns false
    /// </summary>
    /// <param name="action">the database action to run</param>
    /// <param name="recordMetrics">metric recorder</param>
    /// <param name="exceptionLogger">Logger for exceptions</params>
    /// <param name="ct">async cancellation token</param>
    /// <returns>true if anything any real work was completed</returns>
    internal async Task<bool> RunUntilCompletionWithDelay(
        Func<ApplicationDbContext, AsyncServiceScope, CancellationToken, Task<bool>> action,
        Action<bool, double> recordMetrics,
        Action<Exception> exceptionLogger,
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

                workDone = await action(db, scope, ct);
                anyWorkDone |= workDone;
                recordMetrics(true, Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                exceptionLogger(ex);
                recordMetrics(false, Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
                break;
            }
            await Task.Delay(TimeSpan.FromSeconds(1), ct);
        } while (workDone);

        return anyWorkDone;
    }
}
