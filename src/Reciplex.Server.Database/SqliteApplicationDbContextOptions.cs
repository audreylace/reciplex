using System.ComponentModel.DataAnnotations;

namespace Recipe.Database;

/// <summary>
/// Options for database connection
/// </summary>
public class SqliteApplicationDbContextOptions
{
    /// <summary>
    /// Section path
    /// </summary>
    public const string SectionPath = "Reciplex:Sqlite";

    /// <summary>
    /// Database connection string
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string DatabaseConnection { get; set; } = "";

    /// <summary>
    /// If the sqlite3 database driver is enabled
    /// </summary>
    public bool Enable { get; set; }
}
