namespace Reciplex.Server.Database.Results;

public class DatabaseResultVariant<T0, T1, T2> : IDatabaseResult
    where T0 : IDatabaseResult
    where T1 : IDatabaseResult
    where T2 : IDatabaseResult
{
    private DatabaseResultVariant(IDatabaseResult result)
    {
        Result = result;
    }

    public IDatabaseResult Result { get; private init; }

    public static implicit operator DatabaseResultVariant<T0, T1, T2>(T0 t) => new(t);

    public static implicit operator DatabaseResultVariant<T0, T1, T2>(T1 t) => new(t);

    public static implicit operator DatabaseResultVariant<T0, T1, T2>(T2 t) => new(t);
}
