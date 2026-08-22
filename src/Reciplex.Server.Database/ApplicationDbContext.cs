using Microsoft.EntityFrameworkCore;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database;

/// <summary>
/// EF Core database context for the Recipe Application
/// </summary>
/// <remarks>
/// Constructor for ASP.NET
/// </remarks>
/// <param name="options">Application settings</param>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    /// <summary>
    /// User DB objects
    /// </summary>
    public DbSet<UserDbObject> Users { get; set; }

    /// <summary>
    /// Recipe book DB objects
    /// </summary>
    public DbSet<RecipeBookDbObject> RecipeBooks { get; set; }

    /// <summary>
    /// Recipe DB objects
    /// </summary>
    public DbSet<RecipeDbObject> Recipes { get; set; }

    /// <summary>
    /// Set of access entries for recipe books
    /// </summary>
    public DbSet<AdditionalBookUserAccessDbObject> RecipeBookAccessEntries { get; set; }

    /// <summary>
    /// Set recipe book search index tracking records
    /// </summary>
    public DbSet<RecipeBookExternalSearchIndexStateDbObject> RecipeBookExternalSearchIndices { get; set; }

    /// <summary>
    /// Set of recipe search index tracking records
    /// </summary>
    public DbSet<RecipeExternalSearchIndexStateDbObject> RecipeExternalSearchIndices { get; set; }

    /// <summary>
    /// Set of record change entries
    /// </summary>
    public DbSet<RecordDbObjectChangeEntry> RecordChangeQueue { get; set; }
}
