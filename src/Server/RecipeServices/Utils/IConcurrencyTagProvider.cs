namespace Reciplex.Server.RecipeServices.Utils;

public interface IConcurrencyTagProvider
{
    string Next();
}
