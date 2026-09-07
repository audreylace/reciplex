using System.Globalization;

namespace Reciplex.Server.Database.SearchExporter;

/// <summary>
/// Static methods for converting long ids to string for Meilisearch
/// </summary>
static class RecordIdAsStringForSearch
{
    /// <summary>
    /// Creates a recipe key for search
    /// </summary>
    /// <param name="id">the id to convert</param>
    /// <returns>the recipe key as a string</returns>
    public static string MakeRecipeStringKey(long id)
    {
        return $"recipe{PaddedLong(id)}";
    }

    /// <summary>
    /// Creates a recipe book key for search
    /// </summary>
    /// <param name="id">the id to convert</param>
    /// <returns>the recipe book key as a string</returns>
    public static string MakeRecipeBookStringKey(long id)
    {
        return $"recipeBook{PaddedLong(id)}";
    }

    /// <summary>
    /// Creates a string padded to 19 places
    /// </summary>
    /// <param name="id">the long to pad</param>
    /// <returns>the padded long as a string</returns>
    public static string PaddedLong(long id)
    {
        return id.ToString("D19", CultureInfo.InvariantCulture);
    }
}
