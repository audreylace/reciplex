namespace Reciplex.Server.Abstractions.ConcurrencyTagProvider;

public interface IConcurrencyTagProvider
{
    string Next();
}
