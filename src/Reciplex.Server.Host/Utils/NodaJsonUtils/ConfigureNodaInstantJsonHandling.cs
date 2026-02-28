using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Reciplex.Server.Host.Utils.NodaJsonUtils;

public class ConfigureNodaInstantJsonHandling : IConfigureOptions<JsonOptions>
{
    public void Configure(JsonOptions options)
    {
        options.SerializerOptions.Converters.Add(new NodaInstantJsonConverter());
    }
}
