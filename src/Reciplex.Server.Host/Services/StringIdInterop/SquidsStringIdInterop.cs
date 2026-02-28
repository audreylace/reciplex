namespace Reciplex.Server.Host.Services.StringIdInterop;

public class SquidsStringIdInterop(Sqids.SqidsEncoder<long> sqidsEncoder) : IStringIdInterop
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
