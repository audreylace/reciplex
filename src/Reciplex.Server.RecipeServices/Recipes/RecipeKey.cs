namespace Reciplex.Server.RecipeServices.Recipes;

/// <summary>
/// Key uniquely identifying a recipe
/// </summary>
/// <param name="SurrogateKey">The unique surrogate key used internally by the database system</param>
public readonly record struct RecipeKey(long SurrogateKey);
