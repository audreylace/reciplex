using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// Identifies an identity for a (<see cref="Authority"/>, <see cref="Subject"/>) pair.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Has clustered index on (<see cref="Authority"/>, <see cref="Subject"/>) for OIDC lookup. This is not unique.</item>
/// <item>Has clustered index on (<see cref="IsDeleted"/>, <see cref="NextDeletePoll"/>) for user cleanup queue</item>
/// <item>Has index on <see cref="IsDeleted"/> to quickly determine if a user is deleted</item>
/// </list>
/// </remarks>
[Index(nameof(Authority), nameof(Subject), IsUnique = false)]
[Index(nameof(IsDeleted), nameof(NextDeletePoll))]
[Index(nameof(IsDeleted))]
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
    /// True if the user is deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp of deletion
    /// </summary>
    public long? DeletedTime { get; set; }

    /// <summary>
    /// When the user was modified
    /// </summary>
    public required long LastModified { get; set; }

    /// <summary>
    /// When the user was created
    /// </summary>
    public required long Created { get; init; }

    /// <summary>
    /// Recipes shared with this user
    /// </summary>
    public ICollection<AdditionalBookUserAccessDbObject> RecipeBookAccessEntities { get; set; } =
    [];

    /// <summary>
    /// Books the user owns
    /// </summary>
    public ICollection<RecipeBookDbObject> BooksTheUserOwns { get; set; } = [];

    /// <summary>
    /// Next timestamp to check for user deletion cleanup. Defaults to 0 to avoid issues
    /// with different databases sorting null to start or end.
    /// </summary>
    public long NextDeletePoll { get; set; }

    /// <summary>
    /// Counter used to increasing backoff. Defaults to 0 to avoid issues
    /// with different databases sorting null to start or end.
    /// </summary>
    public long NextDeletePollCounter { get; set; }
}
