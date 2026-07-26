namespace Reciplex.Server.Database.Results;

public class DatabaseResultVariant<T0, T1, T2, T3, T4, T5> : IDatabaseResult
    where T0 : IDatabaseResult
    where T1 : IDatabaseResult
    where T2 : IDatabaseResult
    where T3 : IDatabaseResult
    where T4 : IDatabaseResult
    where T5 : IDatabaseResult
{
    private DatabaseResultVariant(IDatabaseResult result)
    {
        Result = result;
    }

    public IDatabaseResult Result { get; private init; }

    public static implicit operator DatabaseResultVariant<T0, T1, T2, T3, T4, T5>(T0 t) => new(t);

    public static implicit operator DatabaseResultVariant<T0, T1, T2, T3, T4, T5>(T1 t) => new(t);

    public static implicit operator DatabaseResultVariant<T0, T1, T2, T3, T4, T5>(T2 t) => new(t);

    public static implicit operator DatabaseResultVariant<T0, T1, T2, T3, T4, T5>(T3 t) => new(t);

    public static implicit operator DatabaseResultVariant<T0, T1, T2, T3, T4, T5>(T4 t) => new(t);

    public static implicit operator DatabaseResultVariant<T0, T1, T2, T3, T4, T5>(T5 t) => new(t);
}
