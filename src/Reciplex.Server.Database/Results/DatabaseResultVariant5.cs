namespace Reciplex.Server.Database.Results;

public class DatabaseResultVariant<T0, T1, T2, T3, T4> : IDatabaseResult
    where T0 : IDatabaseResult
    where T1 : IDatabaseResult
    where T2 : IDatabaseResult
    where T3 : IDatabaseResult
    where T4 : IDatabaseResult
{
    private DatabaseResultVariant(IDatabaseResult result)
    {
        Result = result;
    }

    public IDatabaseResult Result { get; private init; }

    public static implicit operator DatabaseResultVariant<T0, T1, T2, T3, T4>(T0 t) => new(t);

    public static implicit operator DatabaseResultVariant<T0, T1, T2, T3, T4>(T1 t) => new(t);

    public static implicit operator DatabaseResultVariant<T0, T1, T2, T3, T4>(T2 t) => new(t);

    public static implicit operator DatabaseResultVariant<T0, T1, T2, T3, T4>(T3 t) => new(t);

    public static implicit operator DatabaseResultVariant<T0, T1, T2, T3, T4>(T4 t) => new(t);
}
