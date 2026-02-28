using System.Security.Cryptography;

namespace Reciplex.Server.RecipeServices.Utils;

public class RandomNumberGeneratorConcurrencyTagProvider : IConcurrencyTagProvider
{
    public string Next()
    {
        return RandomNumberGenerator.GetHexString(32);
    }
}
