using System.ComponentModel.DataAnnotations;

namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// Top level user db object
/// </summary>
public class UserDbObject
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Display name, not unique
    /// </summary>
    [MaxLength(63)]
    public string DisplayName { get; set; } = "";
}
