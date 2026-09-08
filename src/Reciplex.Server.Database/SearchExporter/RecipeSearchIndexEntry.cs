using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.SearchExporter;

/// <summary>
/// Search index entry for recipes
/// </summary>
public class RecipeSearchIndexEntry
{
    /// <summary>
    /// The id of this search index entry. This is
    /// the base 10 encoding of <see cref="RecipeDbObject.Id"/>.
    /// </summary>
    public required string RecipeId { get; set; }

    /// <summary>
    /// The name of the recipe
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The recipe short description
    /// </summary>
    public required string ShortDescription { get; set; }

    /// <summary>
    /// The main parent book id encoded in base 10 derived from <see cref="RecipeBookDbObject.Id"/>
    /// mapped via <see cref="RecipeDbObject.RecipeBook"/>.
    /// </summary>
    public required string RecipeBookId { get; set; }
}
