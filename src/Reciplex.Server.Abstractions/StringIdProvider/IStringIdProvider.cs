namespace Reciplex.Server.Abstractions.StringIdProvider;

/// <summary>
/// Encodes and decodes keys in the model from strings
/// </summary>
public interface IStringIdProvider
{
    /// <summary>
    /// The model id to encode
    /// </summary>
    /// <param name="id">the id to encode</param>
    /// <returns>the encoded id</returns>
    public string AsString(long id);

    /// <summary>
    /// The model id to decode
    /// </summary>
    /// <param name="id">the id string to decode</param>
    /// <returns>The decoded long or null if <paramref name="id"/> does not decode to a valid long</returns>
    public long? AsLong(string id);
}
