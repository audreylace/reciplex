using Microsoft.Extensions.Options;
using Reciplex.Server.Abstractions.StringIdProvider;
using Sqids;

namespace Reciplex.Server.Host;

public static class AppShortIdWebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddShortIds(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<AppShortIdOptions>(
            builder.Configuration.GetSection(AppShortIdOptions.SectionPath)
        );
        builder.Services.AddSingleton<IStringIdProvider, SquidsStringIdProvider>();
        builder.Services.AddSingleton(provider =>
        {
            IOptions<AppShortIdOptions> options = provider.GetRequiredService<
                IOptions<AppShortIdOptions>
            >();

            SqidsOptions sqidsOptions = new();
            sqidsOptions.MinLength = options.Value.MinLength ?? sqidsOptions.MinLength;
            sqidsOptions.Alphabet = options.Value.Alphabet ?? sqidsOptions.Alphabet;

            if (options.Value.Banned is not null)
            {
                foreach (string word in options.Value.Banned)
                {
                    sqidsOptions.BlockList.Add(word);
                }
            }

            return new SqidsEncoder<long>(sqidsOptions);
        });

        return builder;
    }
}
