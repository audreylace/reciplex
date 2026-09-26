using System.ComponentModel.DataAnnotations;

namespace Reciplex.Server.Database;

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

    /// <summary>
    /// Set to true to enable migrations
    /// </summary>
    public bool EnableMigrations { get; set; }

    /// <summary>
    /// Set to true to use the ensure creation variant to setup the database instead of enable migrations.
    /// Intended for testing only.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Naming",
        "CA1707:Identifiers should not contain underscores",
        Justification = "Ensure obvious in the json"
    )]
    public bool Dangerous_UseEnsureCreation { get; set; }
}
