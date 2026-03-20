using System.Data;
using System.Runtime.CompilerServices;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Abstractions.StringIdProvider;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database.UsersDomain;

/// <summary>
/// Implements <see cref="IUsersRepository"/>
/// </summary>
/// <param name="applicationDbContext">the database connection</param>
/// <param name="concurrencyTagProvider">provider for generating concurrency tags</param>
/// <param name="clock">clock for getting time</param>
public class UsersRepository(
    ApplicationDbContext applicationDbContext,
    IConcurrencyTagProvider concurrencyTagProvider,
    IStringIdProvider stringIdProvider,
    IClock clock
) : IUsersRepository
{
    /// <inheritdoc />
    public async Task<CreateUserResult> CreateUserAsync(CreateUserArgs args, CancellationToken ct)
    {
        CreateUserArgsValidator validator = new();
        ValidationResult validationResult = validator.Validate(args);
        if (!validationResult.IsValid)
        {
            return new CreateUserResult.ValidationFailure(validationResult.ToDictionary());
        }

        long now = Now();
        UserDbObject u = new()
        {
            ConcurrencyTag = NextConcurrencyTag(),
            LastModified = now,
            Created = now,
            DisplayName = args.DisplayName,
            Subject = args.Subject,
            Authority = args.Authority,
        };

        applicationDbContext.Add(u);
        await applicationDbContext.SaveChangesAsync(ct);

        return new CreateUserResult.Success(
            new()
            {
                Id = ToStringKey(u.Id),
                DisplayName = u.DisplayName,
                ConcurrencyTag = u.ConcurrencyTag,
            }
        );
    }

    /// <inheritdoc />
    public async Task<DeleteUserResult> DeleteUserAsync(
        string userKey,
        string concurrencyToken,
        CancellationToken ct
    )
    {
        long userId = FromStringKey(userKey);
        UserDbObject? userRecord = await applicationDbContext
            .Users.Where(u => u.Id == userId)
            .UserNotDeleted()
            .FirstOrDefaultAsync(ct);

        if (userRecord is null)
        {
            return new DeleteUserResult.Success();
        }

        if (userRecord.ConcurrencyTag != concurrencyToken)
        {
            return new DeleteUserResult.Conflict();
        }

        long now = Now();
        userRecord.ConcurrencyTag = NextConcurrencyTag();
        userRecord.LastModified = now;
        userRecord.Deleted = now;

        try
        {
            await applicationDbContext.SaveChangesAsync(ct);
        }
        catch (DBConcurrencyException)
        {
            return new DeleteUserResult.Conflict();
        }

        return new DeleteUserResult.Success();
    }

    /// <inheritdoc />
    public async Task<UserDao?> GetUserAsync(string userKey, CancellationToken cancellationToken)
    {
        long userId = FromStringKey(userKey);
        return await applicationDbContext
            .Users.Where(u => u.Id == userId)
            .UserNotDeleted()
            .Select(u => new UserDao()
            {
                Id = userKey,
                DisplayName = u.DisplayName,
                ConcurrencyTag = u.ConcurrencyTag,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<UserDao> GetUsersBySubjectAsync(
        string authority,
        string subject,
        [EnumeratorCancellation] CancellationToken ct = default
    )
    {
        await foreach (
            var userRow in applicationDbContext
                .Users.Where(u => u.Authority == authority && u.Subject == subject)
                .UserNotDeleted()
                .Select(u => new
                {
                    u.DisplayName,
                    u.ConcurrencyTag,
                    u.Id,
                })
                .AsAsyncEnumerable()
                .WithCancellation(ct)
        )
        {
            yield return new UserDao()
            {
                Id = ToStringKey(userRow.Id),
                DisplayName = userRow.DisplayName,
                ConcurrencyTag = userRow.ConcurrencyTag,
            };
        }
    }

    /// <inheritdoc />
    public async Task<AuthorizationCheckResult> CheckAuthorizationAsync(
        string authority,
        string subject,
        string userKey,
        CancellationToken ct
    )
    {
        long userId = FromStringKey(userKey);
        var user = await applicationDbContext
            .Users.Where(u => u.Id == userId)
            .UserNotDeleted()
            .Select(u => new
            {
                u.Authority,
                u.Subject,
                u.ConcurrencyTag,
            })
            .FirstOrDefaultAsync(ct);

        if (user is null)
        {
            return new AuthorizationCheckResult.NotFound();
        }

        if (user.Authority != authority || user.Subject != subject)
        {
            return new AuthorizationCheckResult.Forbidden(user.ConcurrencyTag);
        }

        return new AuthorizationCheckResult.Authorized(user.ConcurrencyTag);
    }

    /// <inheritdoc />
    public async Task<UpdateUserResult> UpdateUserAsync(
        string userKey,
        string concurrencyToken,
        UpdateUserArgs args,
        CancellationToken ct
    )
    {
        UpdateUserArgsValidator validator = new();
        ValidationResult validationResult = validator.Validate(args);

        if (!validationResult.IsValid)
        {
            return new UpdateUserResult.ValidationFailure(validationResult.ToDictionary());
        }
        long userId = FromStringKey(userKey);
        UserDbObject? u = await applicationDbContext
            .Users.Where(u => u.Id == userId)
            .UserNotDeleted()
            .FirstOrDefaultAsync(ct);

        if (u is null)
        {
            return new UpdateUserResult.NotFound();
        }

        if (u.ConcurrencyTag != concurrencyToken)
        {
            return new UpdateUserResult.Conflict();
        }

        long now = Now();
        u.ConcurrencyTag = NextConcurrencyTag();
        u.LastModified = now;
        u.DisplayName = args.DisplayName;

        try
        {
            await applicationDbContext.SaveChangesAsync(ct);
        }
        catch (DBConcurrencyException)
        {
            return new UpdateUserResult.Conflict();
        }

        return new UpdateUserResult.Success(
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

    /// <summary>
    /// Gets the next concurrency token
    /// </summary>
    /// <returns>the token</returns>
    private string NextConcurrencyTag()
    {
        return concurrencyTagProvider.Next();
    }

    private long FromStringKey(string key)
    {
        return stringIdProvider.AsLong(key)
            ?? throw new ArgumentException($"{key} does not map to a valid long", nameof(key));
    }

    private string ToStringKey(long id)
    {
        return stringIdProvider.AsString(id);
    }
}
