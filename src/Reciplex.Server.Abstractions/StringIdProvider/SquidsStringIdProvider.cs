namespace Reciplex.Server.Abstractions.StringIdProvider;

/// <summary>
/// Implements <see cref="IStringIdProvider" /> using the application registered
/// <see cref="Sqids.SqidsEncoder{T}"/>
/// </summary>
/// <param name="sqidsEncoder">the registered encoder</param>
public class SquidsStringIdProvider(Sqids.SqidsEncoder<long> sqidsEncoder) : IStringIdProvider
{
    public long? AsLong(string id)
    {
        var ids = sqidsEncoder.Decode(id);
        if (ids.Count != 1)
        {
            return null;
        }

        return ids[0];
    }

    public string AsString(long id)
    {
        return sqidsEncoder.Encode(id);
    }
}
