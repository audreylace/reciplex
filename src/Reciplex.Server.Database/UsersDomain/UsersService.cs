using System.Data;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Abstractions.StringIdProvider;
using Reciplex.Server.Database.DbObjects;
using Reciplex.Server.Database.Results;

namespace Reciplex.Server.Database.UsersDomain;

/// <summary>
/// Implements <see cref="IUsersService"/>
/// </summary>
/// <param name="applicationDbContext">the database connection</param>
/// <param name="concurrencyTagProvider">provider for generating concurrency tags</param>
/// <param name="clock">clock for getting time</param>
public class UsersService(
    ApplicationDbContext applicationDbContext,
    IConcurrencyTagProvider concurrencyTagProvider,
    IStringIdProvider stringIdProvider,
    IClock clock
) : IUsersService
{
    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<SuccessResult<UserDao>, ValidationFailureResult>
    > CreateUserAsync(CreateUserArgs args, CancellationToken ct)
    {
        CreateUserArgsValidator validator = new();
        ValidationResult validationResult = await validator.ValidateAsync(args, ct);
        if (!validationResult.IsValid)
        {
            return new ValidationFailureResult(validationResult.ToDictionary());
        }

        long now = Now();
        UserDbObject u = new()
        {
            ConcurrencyTag = concurrencyTagProvider.NextTag(),
            LastModified = now,
            Created = now,
            DisplayName = args.DisplayName,
            Subject = args.Subject,
            Authority = args.Authority,
        };

        applicationDbContext.Add(u);
        await applicationDbContext.SaveChangesAsync(ct);

        return new SuccessResult<UserDao>(
            new()
            {
                Id = stringIdProvider.AsString(u.Id),
                DisplayName = u.DisplayName,
                ConcurrencyTag = u.ConcurrencyTag,
            }
        );
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<EmptySuccessResult, ConflictResult, UserNotFoundResult>
    > DeleteUserAsync(string userKey, string concurrencyToken, CancellationToken ct)
    {
        if (!stringIdProvider.TryParseStringKey(userKey, out long userId))
        {
            return new UserNotFoundResult(userKey);
        }

        UserDbObject? userRecord = await applicationDbContext
            .Users.WithId(userId)
            .UserNotDeleted()
            .FirstOrDefaultAsync(ct);

        if (userRecord is null || userRecord.Deleted != null)
        {
            return new UserNotFoundResult(userKey);
        }

        if (userRecord.ConcurrencyTag != concurrencyToken)
        {
            return new ConflictResult();
        }

        long now = Now();
        userRecord.ConcurrencyTag = concurrencyTagProvider.NextTag();
        userRecord.LastModified = now;
        userRecord.Deleted = now;

        try
        {
            await applicationDbContext.SaveChangesAsync(ct);
        }
        catch (DBConcurrencyException)
        {
            return new ConflictResult();
        }

        return new EmptySuccessResult();
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<SuccessResult<UserDao>, UserNotFoundResult>
    > GetUserAsync(string userKey, CancellationToken cancellationToken)
    {
        if (!stringIdProvider.TryParseStringKey(userKey, out long userId))
        {
            return new UserNotFoundResult(userKey);
        }

        var result = await applicationDbContext
            .Users.AsNoTracking()
            .WithId(userId)
            .UserNotDeleted()
            .Select(u => new UserDao()
            {
                Id = userKey,
                DisplayName = u.DisplayName,
                ConcurrencyTag = u.ConcurrencyTag,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return new UserNotFoundResult(userKey);
        }
        return new SuccessResult<UserDao>(result);
    }

    /// <inheritdoc />
    public async Task<List<UserDao>> GetUsersBySubjectAsync(
        string authority,
        string subject,
        CancellationToken ct
    )
    {
        return await applicationDbContext
            .Users.AsNoTracking()
            .Where(u => u.Authority == authority && u.Subject == subject)
            .UserNotDeleted()
            .Select(userRow => new UserDao()
            {
                Id = stringIdProvider.AsString(userRow.Id),
                DisplayName = userRow.DisplayName,
                ConcurrencyTag = userRow.ConcurrencyTag,
            })
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<SuccessResult<UserDao>, UserNotFoundResult, ForbiddenResult>
    > CheckAuthorizationAsync(
        string authority,
        string subject,
        string userKey,
        CancellationToken ct
    )
    {
        if (!stringIdProvider.TryParseStringKey(userKey, out long userId))
        {
            return new UserNotFoundResult(userKey);
        }

        var user = await applicationDbContext
            .Users.AsNoTracking()
            .WithId(userId)
            .UserNotDeleted()
            .FirstOrDefaultAsync(ct);

        if (user is null)
        {
            return new UserNotFoundResult(userKey);
        }

        if (user.Authority != authority || user.Subject != subject)
        {
            return new ForbiddenResult();
        }

        return new SuccessResult<UserDao>(
            new()
            {
                Id = userKey,
                DisplayName = user.DisplayName,
                ConcurrencyTag = user.ConcurrencyTag,
            }
        );
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<
            SuccessResult<UserDao>,
            ValidationFailureResult,
            UserNotFoundResult,
            ConflictResult
        >
    > UpdateUserAsync(
        string userKey,
        string concurrencyToken,
        UpdateUserArgs args,
        CancellationToken ct
    )
    {
        if (!stringIdProvider.TryParseStringKey(userKey, out long userId))
        {
            return new UserNotFoundResult(userKey);
        }

        UpdateUserArgsValidator validator = new();
        ValidationResult validationResult = await validator.ValidateAsync(args, ct);

        if (!validationResult.IsValid)
        {
            return new ValidationFailureResult(validationResult.ToDictionary());
        }

        UserDbObject? u = await applicationDbContext
            .Users.WithId(userId)
            .UserNotDeleted()
            .FirstOrDefaultAsync(ct);

        if (u is null || u.Deleted != null)
        {
            return new UserNotFoundResult(userKey);
        }

        if (u.ConcurrencyTag != concurrencyToken)
        {
            return new ConflictResult();
        }

        long now = Now();
        u.ConcurrencyTag = concurrencyTagProvider.NextTag();
        u.LastModified = now;
        u.DisplayName = args.DisplayName;

        try
        {
            await applicationDbContext.SaveChangesAsync(ct);
        }
        catch (DBConcurrencyException)
        {
            return new ConflictResult();
        }

        return new SuccessResult<UserDao>(
            new()
            {
                Id = userKey,
                DisplayName = u.DisplayName,
                ConcurrencyTag = u.ConcurrencyTag,
            }
        );
    }

    /// <summary>
    /// Now as a long 64 but unix time stamp with second resolution
    /// </summary>
    /// <returns>the value</returns>
    private long Now()
    {
        return clock.GetCurrentInstant().ToUnixTimeSeconds();
    }
}
