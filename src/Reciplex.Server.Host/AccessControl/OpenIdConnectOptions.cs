using System.ComponentModel.DataAnnotations;

namespace Reciplex.Server.Host.AccessControl;

/// <summary>
/// Open ID Connect settings for the application
/// </summary>
public class OpenIdConnectOptions
{
    /// <summary>
    /// Section path
    /// </summary>
    public const string SectionPath = "Reciplex:OpenIdConnect";

    /// <summary>
    /// Set to true to enable OIDC authentication
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The OIDC authority
    /// </summary>
    [Required]
    public string Authority { get; set; } = "";

    /// <summary>
    /// ID identifying this server to the <see cref="Authority"/>
    /// </summary>
    [Required]
    public string ClientId { get; set; } = "";

    /// <summary>
    /// Shared secret between the client and the <see cref="Authority"/>
    /// </summary>
    /// <remarks>
    /// TODO: Storing this in the settings json is terrible security practice.
    /// Do this in a more secure way.
    /// </remarks>
    [Required]
    public string ClientSecret { get; set; } = "";

    /// <summary>
    /// Disables HTTPs for communication with the OIDC server.
    /// Don't enable in production.
    /// </summary>
    public bool DisableHttps { get; set; }
}
