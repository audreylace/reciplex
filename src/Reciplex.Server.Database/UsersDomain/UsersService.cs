using System.Data;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Abstractions.StringIdProvider;
using Reciplex.Server.Database.DbObjects;
using Reciplex.Server.Database.Results;

namespace Reciplex.Server.Database.UsersDomain;

/// <summary>
/// Implements <see cref="IUsersService"/>
/// </summary>
/// <param name="dbFactory">database connection factory</param>
/// <param name="concurrencyTagProvider">provider for generating concurrency tags</param>
/// <param name="stringIdProvider">provider for marshalling user keys to and from strings</param>
/// <param name="userDeletionNotify">service for notifying the background deletion worker to wake up</param>
/// <param name="clock">clock for getting time</param>
public class UsersService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IConcurrencyTagProvider concurrencyTagProvider,
    IStringIdProvider stringIdProvider,
    IUserDeletionSignal userDeletionNotify,
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

        long now = clock.NowUtcUnixSeconds();
        UserDbObject u = new()
        {
            ConcurrencyTag = concurrencyTagProvider.NextTag(),
            LastModified = now,
            Created = now,
            DisplayName = args.DisplayName,
            Subject = args.Subject,
            Authority = args.Authority,
        };

        await using ApplicationDbContext applicationDbContext =
            await dbFactory.CreateDbContextAsync(ct);

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
    > QueueUserDeleteAsync(string userKey, string concurrencyToken, CancellationToken ct)
    {
        if (!stringIdProvider.TryParseStringKey(userKey, out long userId))
        {
            return new UserNotFoundResult(userKey);
        }

        await using ApplicationDbContext applicationDbContext =
            await dbFactory.CreateDbContextAsync(ct);

        UserDbObject? userRecord = await applicationDbContext
            .Users.WithId(userId)
            .UserNotDeleted()
            .FirstOrDefaultAsync(ct);

        if (userRecord is null || userRecord.IsDeleted)
        {
            return new UserNotFoundResult(userKey);
        }

        if (userRecord.ConcurrencyTag != concurrencyToken)
        {
            return new ConflictResult();
        }

        long now = clock.NowUtcUnixSeconds();
        userRecord.ConcurrencyTag = concurrencyTagProvider.NextTag();
        userRecord.LastModified = now;
        userRecord.IsDeleted = true;
        userRecord.DeletedTime = now;

        try
        {
            await applicationDbContext.SaveChangesAsync(ct);
        }
        catch (DBConcurrencyException)
        {
            return new ConflictResult();
        }

        userDeletionNotify.SignalDeletion();

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

        await using ApplicationDbContext applicationDbContext =
            await dbFactory.CreateDbContextAsync(cancellationToken);
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
        await using ApplicationDbContext applicationDbContext =
            await dbFactory.CreateDbContextAsync(ct);
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

        await using ApplicationDbContext applicationDbContext =
            await dbFactory.CreateDbContextAsync(ct);
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

        await using ApplicationDbContext applicationDbContext =
            await dbFactory.CreateDbContextAsync(ct);
        UserDbObject? u = await applicationDbContext
            .Users.WithId(userId)
            .UserNotDeleted()
            .FirstOrDefaultAsync(ct);

        if (u is null || u.IsDeleted)
        {
            return new UserNotFoundResult(userKey);
        }

        if (u.ConcurrencyTag != concurrencyToken)
        {
            return new ConflictResult();
        }

        long now = clock.NowUtcUnixSeconds();
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

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<EmptySuccessResult, SuccessResult<UserDao>>
    > GetNextUserQueuedForDeletionAsync(CancellationToken ct)
    {
        long now = clock.NowUtcUnixSeconds();
        await using ApplicationDbContext applicationDbContext =
            await dbFactory.CreateDbContextAsync(ct);
        var queryData = await applicationDbContext
            .Users.Where(u => u.IsDeleted && u.NextDeletePoll < now)
            .Select(u => new
            {
                u.Id,
                u.DisplayName,
                u.ConcurrencyTag,
            })
            .FirstOrDefaultAsync(ct);

        if (queryData is null)
        {
            return new EmptySuccessResult();
        }

        return new SuccessResult<UserDao>(
            new()
            {
                Id = stringIdProvider.AsString(queryData.Id),
                DisplayName = queryData.DisplayName,
                ConcurrencyTag = queryData.ConcurrencyTag,
            }
        );
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<EmptySuccessResult, ConflictResult, ValidationFailureResult>
    > RequeueUserForDeletionAsync(string userKey, string concurrencyToken, CancellationToken ct)
    {
        if (!stringIdProvider.TryParseStringKey(userKey, out long userId))
        {
            return BadUserKey();
        }

        long now = clock.NowUtcUnixSeconds();
        await using ApplicationDbContext applicationDbContext =
            await dbFactory.CreateDbContextAsync(ct);
        var queryData = await applicationDbContext
            .Users.Where(u => u.IsDeleted && u.Id == userId && u.ConcurrencyTag == concurrencyToken)
            .Select(u => new { u.NextDeletePoll, u.NextDeletePollCounter })
            .FirstOrDefaultAsync(ct);

        if (queryData is null)
        {
            return new ConflictResult();
        }

        // otherwise work is still pending revisit book with backoff
        var nextPollCounter = Math.Min(16, queryData.NextDeletePollCounter + 1);
        var nextPollTs = now + (1 << (int)queryData.NextDeletePollCounter);
        var newTag = concurrencyTagProvider.NextTag();

        if (
            await applicationDbContext
                .Users.Where(u =>
                    u.IsDeleted && u.Id == userId && u.ConcurrencyTag == concurrencyToken
                )
                .ExecuteUpdateAsync(
                    p =>
                        p.SetProperty(u => u.NextDeletePoll, nextPollTs)
                            .SetProperty(u => u.LastModified, now)
                            .SetProperty(u => u.ConcurrencyTag, newTag)
                            .SetProperty(u => u.NextDeletePollCounter, nextPollCounter),
                    ct
                ) > 0
        )
        {
            return new EmptySuccessResult();
        }

        return new ConflictResult();
    }

    private static DatabaseResultVariant<
        EmptySuccessResult,
        ConflictResult,
        ValidationFailureResult
    > BadUserKey()
    {
        return new ValidationFailureResult(
            new Dictionary<string, string[]>() { { "userKey", ["invalid user key"] } }
        );
    }

    /// <inheritdoc />
    public async Task<
        DatabaseResultVariant<EmptySuccessResult, ConflictResult, ValidationFailureResult>
    > PurgeUserFromDatabaseAsync(string userKey, string concurrencyToken, CancellationToken ct)
    {
        if (!stringIdProvider.TryParseStringKey(userKey, out long userId))
        {
            return BadUserKey();
        }

        await using ApplicationDbContext applicationDbContext =
            await dbFactory.CreateDbContextAsync(ct);
        try
        {
            if (
                await applicationDbContext
                    .Users.Where(u =>
                        u.IsDeleted && u.Id == userId && u.ConcurrencyTag == concurrencyToken
                    )
                    .ExecuteDeleteAsync(ct) < 1
            )
            {
                return new ConflictResult();
            }
        }
        catch (DbUpdateException)
        {
            return new ConflictResult();
        }

        return new EmptySuccessResult();
    }
}
