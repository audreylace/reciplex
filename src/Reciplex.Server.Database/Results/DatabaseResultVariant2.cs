namespace Reciplex.Server.Database.Results;

/// <summary>
/// Database result with two possible values
/// </summary>
/// <typeparam name="TValue1">first value</typeparam>
/// <typeparam name="TValue2">second value</typeparam>
public class DatabaseResultVariant<TValue1, TValue2> : IDatabaseResult
    where TValue1 : IDatabaseResult
    where TValue2 : IDatabaseResult
{
    /// <summary>
    /// Private constructor. Only implicit construction is allowed.
    /// </summary>
    /// <param name="result">the result to store</param>
    private DatabaseResultVariant(IDatabaseResult result)
    {
        Result = result;
    }

    /// <summary>
    /// Stored result
    /// </summary>
    public IDatabaseResult Result { get; private set; }

    /// <summary>
    /// Constructs a result from <typeparamref name="TValue1"/>
    /// </summary>
    /// <param name="value1">the value</param>
    public static implicit operator DatabaseResultVariant<TValue1, TValue2>(TValue1 value1) =>
        new(value1);

    /// <summary>
    /// Constructs a result from <typeparamref name="TValue2"/>
    /// </summary>
    /// <param name="value2">the value</param>
    public static implicit operator DatabaseResultVariant<TValue1, TValue2>(TValue2 value2) =>
        new(value2);
}
