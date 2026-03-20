using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// Top level user db object
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Has clustered index on (<see cref="Authority"/>, <see cref="Subject"/>)</item>
/// </list>
/// </remarks>
[Index(nameof(Authority), nameof(Subject), IsUnique = false)]
public class UserDbObject
{
    public const int DisplayNameMaxLength = 64;
    public const int SubjectMaxLength = 128;
    public const int AuthorityMaxLength = 1024;

    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Display name, not unique
    /// </summary>
    [MaxLength(DisplayNameMaxLength)]
    public string DisplayName { get; set; } = "";

    /// <summary>
    /// The concurrency tag
    /// </summary>
    [ConcurrencyCheck]
    public required string ConcurrencyTag { get; set; } = "";

    /// <summary>
    /// Subject mapping for this user
    /// </summary>
    [MaxLength(SubjectMaxLength)]
    public required string Subject { get; set; }

    /// <summary>
    /// Authority mapping for this user
    /// </summary>
    [MaxLength(AuthorityMaxLength)]
    public required string Authority { get; set; }

    /// <summary>
    /// Populated if the user is deleted. The value is the time of deletion.
    /// </summary>
    public long? Deleted { get; set; }

    /// <summary>
    /// When the user was modified
    /// </summary>
    public required long LastModified { get; set; }

    /// <summary>
    /// When the user was created
    /// </summary>
    public required long Created { get; init; }
}
