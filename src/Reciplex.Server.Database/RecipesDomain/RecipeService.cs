using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Reciplex.Server.Abstractions.ConcurrencyTagProvider;
using Reciplex.Server.Abstractions.StringIdProvider;
using Reciplex.Server.Database.DbObjects;
using Reciplex.Server.Database.RecipeBooksDomain;

namespace Reciplex.Server.Database.RecipesDomain;

internal class RecipesRepository(
    ApplicationDbContext dbContext,
    IClock clock,
    IConcurrencyTagProvider concurrencyTagProvider,
    RecipeBookDbObjectQuery bookQuery,
    RecipeDbObjectQuery recipeQuery,
    RecipeDbObjectListQuery recipeListQuery,
    IStringIdProvider stringIdProvider
) : IRecipesRepository
{
    /// <inheritdoc />
    public async Task<CreateRecipeResult> CreateRecipeAsync(
        string bookKey,
        string userKey,
        CreateRecipeArgs args,
        CancellationToken cancellationToken
    )
    {
        CreateRecipeArgsValidator validator = new();
        var validationResult = validator.Validate(args);
        if (!validationResult.IsValid)
        {
            return new CreateRecipeResult.ValidationFailure(validationResult.ToDictionary());
        }

        RecipeBookDbObjectQueryResult? bookLookup = await bookQuery.ExecuteQueryAsync(
            FromStringKey(bookKey),
            FromStringKey(userKey),
            cancellationToken
        );

        if (bookLookup is null)
        {
            return new CreateRecipeResult.NotFound();
        }
        if (!bookLookup.MayEdit)
        {
            return new CreateRecipeResult.Forbidden();
        }

        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        RecipeDbObject recipeDbObject = new()
        {
            RecipeBookFk = bookLookup.RecipeBook.Id,
            Name = args.Name,
            ShortDescription = args.ShortDescription,
            Details = args.Details,
            Created = now,
            LastModified = now,
            ConcurrencyTag = concurrencyTagProvider.Next(),
        };
        dbContext.Add(recipeDbObject);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CreateRecipeResult.Success(DbObjectToRecipeDao(recipeDbObject, true));
    }

    public async Task<DeleteRecipeByIdResult> DeleteRecipeAsync(
        string recipeKey,
        string userKey,
        string concurrencyTag,
        CancellationToken cancellationToken
    )
    {
        RecipeDbObjectQueryResult query = await recipeQuery.ExecuteQueryAsync(
            FromStringKey(recipeKey),
            FromStringKey(userKey),
            cancellationToken
        );

        switch (query)
        {
            case RecipeDbObjectQueryResult.NotFound:
                return new DeleteRecipeByIdResult.NotFound();
            case RecipeDbObjectQueryResult.Forbidden:
                return new DeleteRecipeByIdResult.Forbidden();
            case RecipeDbObjectQueryResult.Success success:
                if (success.Recipe.ConcurrencyTag != concurrencyTag)
                {
                    return new DeleteRecipeByIdResult.Conflict();
                }

                long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
                success.Recipe.LastModified = now;
                success.Recipe.Deleted = now;
                success.Recipe.ConcurrencyTag = concurrencyTagProvider.Next();
                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return new DeleteRecipeByIdResult.Conflict();
                }
                return new DeleteRecipeByIdResult.Success();
            default:
                throw new NotImplementedException();
        }
    }

    public async Task<RecipeDao?> GetRecipeAsync(
        string recipeKey,
        string userKey,
        CancellationToken cancellationToken
    )
    {
        RecipeDbObjectQueryResult query = await recipeQuery.ExecuteQueryAsync(
            FromStringKey(recipeKey),
            FromStringKey(userKey),
            cancellationToken
        );

        return query switch
        {
            RecipeDbObjectQueryResult.Forbidden or RecipeDbObjectQueryResult.NotFound => null,
            RecipeDbObjectQueryResult.Success success => DbObjectToRecipeDao(
                success.Recipe,
                success.MayEdit
            ),
            _ => throw new NotImplementedException(),
        };
    }

    public async IAsyncEnumerable<RecipeDao> ListRecipesAsync(
        string userKey,
        ListRecipesArgs args,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (
            var row in recipeListQuery
                .ExecuteQueryAsync(
                    FromStringKey(userKey),
                    bookId: args.RecipeBookId is not null ? FromStringKey(args.RecipeBookId) : null,
                    ordering: args.ResultOrder,
                    idIsBefore: args.BeforeRecipeId is not null
                        ? FromStringKey(args.BeforeRecipeId)
                        : null,
                    idIsAfter: args.AfterRecipeId is not null
                        ? FromStringKey(args.AfterRecipeId)
                        : null,
                    cancellationToken: cancellationToken
                )
                .Take(args.ResultCount)
                .Select(r => DbObjectToRecipeDao(r.Recipe, r.MayEdit))
                .WithCancellation(cancellationToken)
        )
        {
            yield return row;
        }
    }

    public async Task<UpdateRecipeResult> UpdateRecipeAsync(
        string recipeKey,
        string userKey,
        string concurrencyTag,
        UpdateRecipeArgs args,
        CancellationToken cancellationToken
    )
    {
        UpdateRecipeArgsValidator validator = new();
        var validationResult = validator.Validate(args);
        if (!validationResult.IsValid)
        {
            return new UpdateRecipeResult.ValidationFailure(validationResult.ToDictionary());
        }

        RecipeDbObjectQueryResult query = await recipeQuery.ExecuteQueryAsync(
            FromStringKey(recipeKey),
            FromStringKey(userKey),
            cancellationToken
        );

        switch (query)
        {
            case RecipeDbObjectQueryResult.NotFound:
                return new UpdateRecipeResult.NotFound();
            case RecipeDbObjectQueryResult.Forbidden:
                return new UpdateRecipeResult.Forbidden();
            case RecipeDbObjectQueryResult.Success success:

                if (!success.MayEdit)
                {
                    return new UpdateRecipeResult.Forbidden();
                }

                if (success.Recipe.ConcurrencyTag != concurrencyTag)
                {
                    return new UpdateRecipeResult.Conflict();
                }

                success.Recipe.Name = args.Name;
                success.Recipe.ShortDescription = args.ShortDescription;
                success.Recipe.Details = args.Details;
                success.Recipe.LastModified = clock.GetCurrentInstant().ToUnixTimeSeconds();
                success.Recipe.ConcurrencyTag = concurrencyTagProvider.Next();
                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return new UpdateRecipeResult.Conflict();
                }

                return new UpdateRecipeResult.Success(DbObjectToRecipeDao(success.Recipe, true));
            default:
                throw new NotImplementedException();
        }
    }

    private RecipeDao DbObjectToRecipeDao(RecipeDbObject recipeDbObject, bool mayEdit) =>
        new()
        {
            Id = ToStringKey(recipeDbObject.Id),
            Name = recipeDbObject.Name,
            ShortDescription = recipeDbObject.ShortDescription,
            ConcurrencyTag = recipeDbObject.ConcurrencyTag,
            LastModified = Instant.FromUnixTimeSeconds(recipeDbObject.LastModified),
            Created = Instant.FromUnixTimeSeconds(recipeDbObject.Created),
            BookId = ToStringKey(recipeDbObject.RecipeBookFk),
            Details = recipeDbObject.Details,
            MayEdit = mayEdit,
        };

    private long FromStringKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
        return stringIdProvider.AsLong(key)
            ?? throw new ArgumentException($"{key} does not map to a valid long", nameof(key));
    }

    private string ToStringKey(long id)
    {
        return stringIdProvider.AsString(id);
    }
}
