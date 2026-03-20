using System.Diagnostics.CodeAnalysis;
using Reciplex.Server.Database.RecipeBooksDomain;

namespace Reciplex.Server.Host.Models;

public class RecipeBookJsonResponse
{
    public required string BookKey { get; init; }
    public required string Name { get; init; }
    public required string ShortDescription { get; init; }
    public required string OwningUserKey { get; init; }
    public required NodaTime.Instant Created { get; init; }
    public required NodaTime.Instant LastModified { get; init; }
    public required string ConcurrencyTag { get; init; }
    public required bool MayEdit { get; init; }
    public required bool MayDelete { get; init; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public RecipeBookJsonResponse() { }

    /// <summary>
    /// Builds an instance of this object from <paramref name="book"/>
    /// </summary>
    /// <param name="book">the dao data to transfer</param>
    [SetsRequiredMembers]
    public RecipeBookJsonResponse(RecipeBookDao book)
    {
        BookKey = book.Id;
        Name = book.Name;
        ShortDescription = book.ShortDescription;
        OwningUserKey = book.OwningUserKey;
        Created = book.Created;
        LastModified = book.LastModified;
        ConcurrencyTag = book.ConcurrencyTag;
        MayEdit = book.MayEditBook;
        MayDelete = book.MayDeleteBook;
    }
}
