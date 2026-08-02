namespace Reciplex.Server.Database.Results;

/// <summary>
/// Result variant with 6 values
/// </summary>
/// <typeparam name="TValue1">value type 1</typeparam>
/// <typeparam name="TValue2">value type 2</typeparam>
/// <typeparam name="TValue3">value type 3</typeparam>
/// <typeparam name="TValue4">value type 4</typeparam>
/// <typeparam name="TValue5">value type 5</typeparam>
/// <typeparam name="TValue6">value type 6</typeparam>
public class DatabaseResultVariant<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>
    : IDatabaseResult
    where TValue1 : IDatabaseResult
    where TValue2 : IDatabaseResult
    where TValue3 : IDatabaseResult
    where TValue4 : IDatabaseResult
    where TValue5 : IDatabaseResult
    where TValue6 : IDatabaseResult
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
    public static implicit operator DatabaseResultVariant<
        TValue1,
        TValue2,
        TValue3,
        TValue4,
        TValue5,
        TValue6
    >(TValue1 value1) => new(value1);

    /// <summary>
    /// Builds this from <paramref name="value2"/>
    /// </summary>
    /// <param name="value2">the value to box</param>
    public static implicit operator DatabaseResultVariant<
        TValue1,
        TValue2,
        TValue3,
        TValue4,
        TValue5,
        TValue6
    >(TValue2 value2) => new(value2);

    /// <summary>
    /// Builds this from <paramref name="value3"/>
    /// </summary>
    /// <param name="value3">the value to box</param>
    public static implicit operator DatabaseResultVariant<
        TValue1,
        TValue2,
        TValue3,
        TValue4,
        TValue5,
        TValue6
    >(TValue3 value3) => new(value3);

    /// <summary>
    /// Builds this from <paramref name="value4"/>
    /// </summary>
    /// <param name="value4">the value to box</param>
    public static implicit operator DatabaseResultVariant<
        TValue1,
        TValue2,
        TValue3,
        TValue4,
        TValue5,
        TValue6
    >(TValue4 value4) => new(value4);

    /// <summary>
    /// Builds this from <paramref name="value5"/>
    /// </summary>
    /// <param name="value5">the value to box</param>
    public static implicit operator DatabaseResultVariant<
        TValue1,
        TValue2,
        TValue3,
        TValue4,
        TValue5,
        TValue6
    >(TValue5 value5) => new(value5);

    /// <summary>
    /// Builds this from <paramref name="value6"/>
    /// </summary>
    /// <param name="value6">the value to box</param>
    public static implicit operator DatabaseResultVariant<
        TValue1,
        TValue2,
        TValue3,
        TValue4,
        TValue5,
        TValue6
    >(TValue6 value6) => new(value6);
}
