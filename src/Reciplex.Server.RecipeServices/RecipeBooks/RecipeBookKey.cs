namespace Reciplex.Server.RecipeServices.RecipeBooks;

/// <summary>
/// Key uniquely identifying a recipe book
/// </summary>
/// <param name="SurrogateKey">The unique surrogate key used internally by the database system</param>
public readonly record struct RecipeBookKey(long SurrogateKey);
