namespace Reciplex.Server.Host.Options;

/// <summary>
/// Application override Sqids options
/// </summary>
public class AppShortIdOptions
{
    /// <summary>
    /// Section path
    /// </summary>
    public const string SectionPath = "Reciplex:ShortIds";

    /// <summary>
    /// Custom alphabet that will be used for the IDs.
    /// Must contain at least 5 characters.
    /// The default is lowercase letters, uppercase letters, and digits.
    /// </summary>
    public string? Alphabet { get; set; }

    /// <summary>
    /// The minimum length for the IDs.
    /// The default is 0; meaning the IDs will be as short as possible.
    /// 255 is the maximum.
    /// </summary>
    public int? MinLength { get; set; }

    /// <summary>
    /// Additional banned words to merge into the default set
    /// </summary>
    public List<string>? Banned { get; set; } = [];
}
