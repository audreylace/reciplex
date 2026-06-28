namespace Reciplex.Server.Host.AccessControl;

public class OidcOverrideBackchannelRoutingHandler(
    OpenIdConnectOptions openIdConnectConfiguration,
    HttpMessageHandler innerHandler
) : DelegatingHandler(innerHandler)
{
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
