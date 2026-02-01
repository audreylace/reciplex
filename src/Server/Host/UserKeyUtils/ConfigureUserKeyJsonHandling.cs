using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Reciplex.Server.Host.UserKeyUtils;

public class ConfigureUserKeyJsonHandling(IStringUserKeyInterop userKeyInterop)
    : IConfigureOptions<JsonOptions>
{
    public void Configure(JsonOptions options)
    {
        options.SerializerOptions.Converters.Add(new UserKeyJsonConverter(userKeyInterop));
    }
}
