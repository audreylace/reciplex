namespace Reciplex.Server.Abstractions;

/// <summary>
/// A type that runs before app startup. Throwing aborts startup.
/// </summary>
public interface IRunBeforeAppStartup
{
    /// <summary>
    /// Invoked after app build and before run
    /// </summary>
    /// <param name="ct">token that cancels the async operation</param>
    Task RunBeforeStartupAsync(CancellationToken ct);
}
