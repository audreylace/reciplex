using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;
using Meilisearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using Reciplex.Server.Meilisearch;
using Reciplex.Server.Meilisearch.Responses;

namespace Reciplex.Server.Database.SearchExporter;

internal sealed class SearchExporterService(
    IServiceProvider sp,
    ILogger<SearchExporterService> logger,
    IClock clock
) : BackgroundService
{
    /// <summary>
    /// Recipe search index
    /// </summary>
    private const string RecipesSearchIndex = "recipes";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }

    private async Task ExtractChangedRecords(CancellationToken ct)
    {
        await using AsyncServiceScope scope = sp.CreateAsyncScope();
        await using ApplicationDbContext db =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IMeilisearchClient searchClient =
            scope.ServiceProvider.GetRequiredService<IMeilisearchClient>();

        // todo - should we handle orphaned entries - No, probably not. The search
        //        engine will just overwrite the data again.
        var entries = await db
            .RecipeSearchExtractionStatusEntries.AsNoTracking()
            .Where(e =>
                (e.SearchVersion == null || e.Recipe!.SearchVersion != e.SearchVersion)
                && e.Recipe!.Deleted == null
                && e.RecipeBook!.Deleted == null
                && e.RecipeBook!.Owner!.Deleted == null
            )
            .Select(e => new
            {
                e.Id,
                e.RecipeBookFk,
                e.RecipeFk,
                e.Recipe!.Name,
                e.Recipe!.ShortDescription,
                e.Recipe!.SearchVersion,
            })
            .Take(20)
            .ToListAsync(ct);

        if (entries.Count < 1)
        {
            return;
        }

        GetIndexResponse? index = await searchClient.GetIndexAsync(RecipesSearchIndex, ct);
        if (index is null)
        {
            // todo - ? it should have been made ??? should we throw IDK!
            return;
        }

        // todo - deal with exceptions
        var result = await searchClient.UpsertDocumentsAsync(
            RecipesSearchIndex,
            entries.Select(e => new RecipeSearchIndexEntry()
            {
                Id = MakeRecipeStringKey(e.RecipeFk),
                Name = e.Name,
                ShortDescription = e.ShortDescription,
                BookId = MakeRecipeBookStringKey(e.RecipeBookFk),
            }),
            ct
        );

        if (result is null)
        {
            // todo - ? it should have been made ??? should we throw IDK!
            return;
        }

        // todo - database errors? How we deal with that?
        List<long> searchIndexEntries = [.. entries.Select(e => e.Id)];
        long now = clock.GetCurrentInstant().ToUnixTimeSeconds();
        await db
            .RecipeSearchExtractionStatusEntries.Where(e => searchIndexEntries.Contains(e.Id))
            .ExecuteUpdateAsync(
                setter =>
                    setter
                        .SetProperty(e => e.TaskUid, result.TaskUid)
                        .SetProperty(e => e.TaskPostTime, now),
                ct
            );

        while (!ct.IsCancellationRequested)
        {
            TaskStatusResponse taskStatus = await searchClient.GetTaskStatus(result.TaskUid, ct);
            if (taskStatus.Status == MeilisearchTaskStatus.Succeeded)
            {
                foreach (var entry in entries)
                {
                    // todo - database errors? How we deal with that?
                    await db
                        .RecipeSearchExtractionStatusEntries.Where(e =>
                            e.TaskUid == result.TaskUid && e.Id == entry.Id
                        )
                        .ExecuteUpdateAsync(
                            setter =>
                                setter
                                    .SetProperty(e => e.TaskUid, (long?)null)
                                    .SetProperty(e => e.TaskPostTime, (long?)null)
                                    .SetProperty(e => e.SearchVersion, entry.SearchVersion),
                            ct
                        );
                }
                break;
            }
            if (
                taskStatus.Status == MeilisearchTaskStatus.Failed
                || taskStatus.Status == MeilisearchTaskStatus.Canceled
            )
            {
                // todo - database errors? How we deal with that?
                await db
                    .RecipeSearchExtractionStatusEntries.Where(e => e.TaskUid == result.TaskUid)
                    .ExecuteUpdateAsync(
                        setter =>
                            setter
                                .SetProperty(e => e.TaskUid, (long?)null)
                                .SetProperty(e => e.TaskPostTime, (long?)null),
                        ct
                    );
                break; // todo - ? can we inspect the response to figure out which document is bad?
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100), ct); // circuit breaker or something?
        }
    }

    private static string MakeRecipeStringKey(long id)
    {
        return $"recipe{id.ToString("D19", CultureInfo.InvariantCulture)}";
    }

    private static string MakeRecipeBookStringKey(long id)
    {
        return $"recipeBook{id.ToString("D19", CultureInfo.InvariantCulture)}";
    }
}
