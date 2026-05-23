using System.Security.Cryptography;

namespace Reciplex.Server.Abstractions.ConcurrencyTagProvider;

public class RandomNumberGeneratorConcurrencyTagProvider : IConcurrencyTagProvider
{
    public string NextTag()
    {
        return RandomNumberGenerator.GetHexString(32);
    }
}
