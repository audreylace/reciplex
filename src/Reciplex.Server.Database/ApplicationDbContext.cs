using Microsoft.EntityFrameworkCore;
using Reciplex.Server.Database.DbObjects;

namespace Reciplex.Server.Database;

/// <summary>
/// EF Core database context for the Recipe Application
/// </summary>
public class ApplicationDbContext : DbContext
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
    /// Recipe search extraction entries
    /// </summary>
    public DbSet<RecipeSearchWorkerStateDbObject> RecipeSearchExtractionStatusEntries { get; set; }

    /// <summary>
    /// Constructor for ASP.NET
    /// </summary>
    /// <param name="options">Application settings</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
}
