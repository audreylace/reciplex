using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.RecipesDomain;

class RecipeDbObjectListEntry
{
    public required RecipeDbObject Recipe { get; init; }
    public required bool MayEdit { get; init; }
}
