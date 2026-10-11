using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reciplex.Server.Database.SearchIndexDomain;
using Reciplex.Server.Meilisearch;
using Reciplex.Server.SearchExporter.HostedServices;
using Reciplex.Server.SearchExporter.Options;
using Reciplex.Server.SearchExporter.Repositories;
using Reciplex.Server.SearchExporter.SearchBackgroundTasks;

namespace Reciplex.Server.SearchExporter;

/// <summary>
/// Configures application DB access for the program
/// </summary>
public static class WebApplicationBuilderExtensions
{
    public static void ConfigureSearch(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IRecipeMutationNotifyService, RecipeMutationNotifyService>();

        builder.Services.Configure<SearchExporterOptions>(
            builder.Configuration.GetSection(SearchExporterOptions.SectionPath)
        );

        SearchExporterOptions configOptions = new();
        builder.Configuration.GetSection(SearchExporterOptions.SectionPath).Bind(configOptions);

        if (configOptions.Enable)
        {
            builder.Services.AddSingleton<IMeilisearchClient, MeilisearchClient>();
            builder
                .Services.AddHttpClient<IMeilisearchClient, MeilisearchClient>(client =>
                {
                    if (!configOptions.Enable)
                    {
                        return;
                    }

                    client.BaseAddress = new Uri(configOptions.Host);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        configOptions.AuthenticationToken
                    );
                })
                .AddStandardResilienceHandler();

            builder.Services.AddHostedService<RecipeSearchHostedBackgroundService>();

            builder.Services.AddSingleton<
                IRecipeSearchExportStatusRepository,
                RecipeSearchExportStatusRepository
            >();
            builder.Services.AddSingleton<
                ISearchIndexExporterScopeFactory,
                SearchIndexExporterScopeFactory
            >();
            builder.Services.AddScoped<ISearchIndexExporterScope, SearchIndexExporterScope>();
            builder.Services.AddSingleton<ISearchIndexRepository, MeilisearchIndexRepository>();
            builder.Services.AddSingleton<LeaseRenewer>();
            builder.Services.AddSingleton<RecipeSearchIndexExporterStrategy>();
            builder.Services.AddSingleton<ResiliencePipelineBuilderFactory>();
            builder.Services.AddScoped<ISearchBackgroundTask, RecipeDeletionSearchBackgroundTask>();

            builder.Services.AddScoped<
                ISearchBackgroundTask,
                RecipeLeaseBreakerSearchBackgroundTask
            >();
            builder.Services.AddScoped<
                ISearchBackgroundTask,
                ChangedRecipeExporterSearchBackgroundTask
            >();
            builder.Services.AddScoped<
                ISearchBackgroundTask,
                RecipesThatFailToDeleteSearchBackgroundTask
            >();
        }
    }
}
