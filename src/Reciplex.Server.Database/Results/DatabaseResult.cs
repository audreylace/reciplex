namespace Reciplex.Server.Database.Results;

public interface IDatabaseResult;

public record class EmptySuccessResult : IDatabaseResult;

public record class SuccessResult<T>(T Value) : IDatabaseResult;

public record class CreatedResult<T>(T Value) : IDatabaseResult;

public record class NotFoundResult : IDatabaseResult;

public record class ForbiddenResult : IDatabaseResult;

public record class ConflictResult : IDatabaseResult;

/// <summary>
/// User not found
/// </summary>
/// <param name="UserKey">the user key</param>
public record class UserNotFoundResult(string UserKey) : IDatabaseResult;

public record class ValidationFailureResult(IDictionary<string, string[]> Errors) : IDatabaseResult;

public class DatabaseResultVariant<T0, T1> : IDatabaseResult
    where T0 : IDatabaseResult
    where T1 : IDatabaseResult
{
    private DatabaseResultVariant(IDatabaseResult result)
    {
        Result = result;
    }

    public IDatabaseResult Result { get; private set; }

    public static implicit operator DatabaseResultVariant<T0, T1>(T0 t) => new(t);

    public static implicit operator DatabaseResultVariant<T0, T1>(T1 t) => new(t);
}

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

public class DatabaseResultVariant<T0, T1, T2, T3> : IDatabaseResult
    where T0 : IDatabaseResult
    where T1 : IDatabaseResult
    where T2 : IDatabaseResult
    where T3 : IDatabaseResult
{
    private DatabaseResultVariant(IDatabaseResult result)
    {
        Result = result;
    }

    public IDatabaseResult Result { get; private init; }

    public static implicit operator DatabaseResultVariant<T0, T1, T2, T3>(T0 t) => new(t);

    public static implicit operator DatabaseResultVariant<T0, T1, T2, T3>(T1 t) => new(t);

    public static implicit operator DatabaseResultVariant<T0, T1, T2, T3>(T2 t) => new(t);

    public static implicit operator DatabaseResultVariant<T0, T1, T2, T3>(T3 t) => new(t);
}

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
