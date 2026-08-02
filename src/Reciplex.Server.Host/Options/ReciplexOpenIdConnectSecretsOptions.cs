using System.ComponentModel.DataAnnotations;

namespace Reciplex.Server.Host.Options;

/// <summary>
/// Secrets for communicating with the OIDC server. This should be stored in a secrets vault and passed to the application.
/// </summary>
public class ReciplexOpenIdConnectSecretsOptions
{
    /// <summary>
    /// Section path
    /// </summary>
    public const string SectionPath = "Reciplex:OidcSecrets";

    /// <summary>
    /// Shared secret between the client and the <see cref="Authority"/>
    /// </summary>
    [Required]
    public string ClientSecret { get; set; } = "";

    /// <summary>
    /// ID identifying this server to the <see cref="Authority"/>
    /// </summary>
    [Required]
    public string ClientId { get; set; } = "";
}
