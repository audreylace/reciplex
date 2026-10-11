using NodaTime;

namespace Reciplex.Server.Database;

/// <summary>
/// Extensions on Nodatime <see cref="IClock"/>
/// </summary>
internal static class ClockExtensions
{
    /// <summary>
    /// Returns now in UTC time formatted as the number of seconds since the unix epoch in 1970
    /// </summary>
    /// <param name="clock">clock to read value from</param>
    /// <returns>the timestamp</returns>
    public static long NowUtcUnixSeconds(this IClock clock)
    {
        return clock.GetCurrentInstant().ToUnixTimeSeconds();
    }
}
