using System.Net.Http.Metrics;

namespace Reciplex.Server.Host.Metrics;

/// <summary>
/// Labels http client with `reciplex_http.client_name` set to <paramref name="clientName"/>
/// </summary>
/// <param name="clientName">the http client name</param>
public class HttpMetricsClientEnricherHandler(string clientName) : DelegatingHandler
{
    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        // Add custom metric tag to the http.client.request.duration metric
        HttpMetricsEnrichmentContext.AddCallback(
            request,
            context =>
            {
                if (context.Request.RequestUri != null)
                {
                    // Capture the URL path
                    context.AddCustomTag("reciplex_http.client_name", clientName);
                }
            }
        );

        return await base.SendAsync(request, cancellationToken);
    }
}
