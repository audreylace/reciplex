using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Recipe.Database.DbObjects;

/// <summary>
/// A recipe held inside of a <see cref="RecipeBookDbObject"/>
/// </summary>
[Index(nameof(RecipeBookFk))]
public class RecipeDbObject
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Title of the recipe
    /// </summary>
    [MaxLength(127)]
    public string Title { get; set; } = "";

    /// <summary>
    /// Recipe short description
    /// </summary>
    [MaxLength(255)]
    public string ShortDescription { get; set; } = "";

    /// <summary>
    /// The book holding this recipe
    /// </summary>
    [DisallowNull]
    [ForeignKey(nameof(RecipeBookFk))]
    [Required]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public RecipeBookDbObject? RecipeBook { get; set; }

    /// <summary>
    /// Database FK to <see cref="RecipeBookDbObject"/> for property <see cref="RecipeBook"/>
    /// </summary>
    public long RecipeBookFk { get; set; }

    /// <summary>
    /// The stored json model
    /// </summary>
    public RecipeDbJsonObject JsonData { get; set; } = new();
}
