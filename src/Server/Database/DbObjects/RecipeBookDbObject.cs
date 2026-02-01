using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Reciplex.Server.Database.DbObjects;

/// <summary>
/// Recipe book database object
/// </summary>
[Index(nameof(OwnerFk))]
public class RecipeBookDbObject
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Title of the recipe book
    /// </summary>
    [MaxLength(127)]
    public string Title { get; set; } = "";

    /// <summary>
    /// Recipe book short description
    /// </summary>
    [MaxLength(255)]
    public string ShortDescription { get; set; } = "";

    /// <summary>
    /// Owner of this book
    /// </summary>
    [DisallowNull]
    [ForeignKey(nameof(OwnerFk))]
    [Required]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public UserDbObject? Owner { get; set; }

    /// <summary>
    /// Database FK to <see cref="UserDbObject"/> for property <see cref="Owner"/>
    /// </summary>
    public long OwnerFk { get; set; }

    /// <summary>
    /// Navigation property to recipes in this book
    /// </summary>
    public ICollection<RecipeDbObject> Recipes { get; set; } = [];

    /// <summary>
    /// Users with additional access
    /// </summary>
    public ICollection<AdditionalBookUserAccessDbObject> AdditionalUsers { get; set; } = [];
}
