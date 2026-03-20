using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipeBooksDomain;

internal class RecipeBookDbObjectQueryResult
{
    public required RecipeBookDbObject RecipeBook { get; init; }
    public required bool MayEdit { get; init; }
    public required bool MayDelete { get; init; }
}
