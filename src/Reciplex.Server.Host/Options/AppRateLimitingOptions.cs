namespace Reciplex.Server.Host.Options;

/// <summary>
/// Application rate limiting options
/// </summary>
public class AppRateLimitingOptions
{
    /// <summary>
    /// Section path
    /// </summary>
    public const string SectionPath = "Reciplex:RateLimiting";

    /// <summary>
    /// Set to true to enable rate limiting
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// The size of the window in seconds
    /// </summary>
    public long AuthUserWindowSeconds { get; set; } = 10;

    /// <summary>
    /// The number of window segments
    /// </summary>
    public int AuthUserWindowSegments { get; set; } = 4;

    /// <summary>
    /// The max number of requests in the window
    /// </summary>
    public int AuthUserMaxRequests { get; set; } = 100;

    /// <summary>
    /// The max number of requests in the window
    /// </summary>
    public int PerIpMaxRequests { get; set; } = 200;

    /// <summary>
    /// The size of the window in seconds
    /// </summary>
    public long PerIpWindowSeconds { get; set; } = 60;

    /// <summary>
    /// Max number of tokens
    /// </summary>
    public int GlobalBucketMaxTokens { get; set; } = 1000;

    /// <summary>
    /// Tokens added to the bucket per period
    /// </summary>
    public int GlobalBucketReplenishRate { get; set; } = 20;

    /// <summary>
    /// The rate of replenishment
    /// </summary>
    public long GlobalBucketReplenishPeriodSeconds { get; set; } = 1;
}
