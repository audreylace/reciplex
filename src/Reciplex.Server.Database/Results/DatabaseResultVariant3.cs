namespace Reciplex.Server.Database.Results;

/// <summary>
/// Database result with 3 possible values
/// </summary>
/// <typeparam name="TValue1">possible value 1</typeparam>
/// <typeparam name="TValue2">possible value 2</typeparam>
/// <typeparam name="TValue3">possible value 3</typeparam>
public class DatabaseResultVariant<TValue1, TValue2, TValue3> : IDatabaseResult
    where TValue1 : IDatabaseResult
    where TValue2 : IDatabaseResult
    where TValue3 : IDatabaseResult
{
    /// <summary>
    /// Private constructor, can only be built through operators
    /// </summary>
    /// <param name="result">the value to store</param>
    private DatabaseResultVariant(IDatabaseResult result)
    {
        Result = result;
    }

    /// <summary>
    /// The held result variant
    /// </summary>
    public IDatabaseResult Result { get; private init; }

    /// <summary>
    /// Builds this from <paramref name="value1"/>
    /// </summary>
    /// <param name="value1">the value to box</param>
    public static implicit operator DatabaseResultVariant<TValue1, TValue2, TValue3>(
        TValue1 value1
    ) => new(value1);

    /// <summary>
    /// Builds this from <paramref name="value2"/>
    /// </summary>
    /// <param name="value2">the value to box</param>
    public static implicit operator DatabaseResultVariant<TValue1, TValue2, TValue3>(
        TValue2 value2
    ) => new(value2);

    /// <summary>
    /// Builds this from <paramref name="value3"/>
    /// </summary>
    /// <param name="value3">the value to box</param>
    public static implicit operator DatabaseResultVariant<TValue1, TValue2, TValue3>(
        TValue3 value3
    ) => new(value3);
}
