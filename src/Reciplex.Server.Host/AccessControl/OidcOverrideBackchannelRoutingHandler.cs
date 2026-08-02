using Reciplex.Server.Host.Options;

namespace Reciplex.Server.Host.AccessControl;

/// <summary>
/// Mutates outbound requests to OIDC server targeting a different server
/// </summary>
/// <param name="openIdConnectConfiguration">oidc server configuration</param>
/// <param name="innerHandler">the inner http handler to wrap</param>
public class OidcOverrideBackChannelRoutingHandler(
    ReciplexOpenIdConnectOptions openIdConnectConfiguration,
    HttpMessageHandler innerHandler
) : DelegatingHandler(innerHandler)
{
    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (request.RequestUri != null)
        {
            Uri uri = new(openIdConnectConfiguration.BackChannelHostOverride);
            var builder = new UriBuilder(request.RequestUri)
            {
                Scheme = uri.Scheme,
                Host = uri.Host,
                Port = uri.Port,
            };
            string originalHost = request.RequestUri.Authority;
            request.RequestUri = builder.Uri;
            request.Headers.Host = originalHost;
        }

        return base.SendAsync(request, cancellationToken);
    }
}
