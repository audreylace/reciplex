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
    public const string SectionPath = "Reciplex:Oidc";

    /// <summary>
    /// Set to true to enable OIDC authentication
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// The OIDC authority
    /// </summary>
    [Required]
    public string Authority { get; set; } = "";

    /// <summary>
    /// Disables HTTPs for communication with the OIDC server.
    /// </summary>
    public bool InsecureDisableHttps { get; set; }

    /// <summary>
    /// Accepts any HTTPs certificate bypassing certificate chain validation
    /// </summary>
    public bool InsecureAcceptAnyServerCertificate { get; set; }

    /// <summary>
    /// Maps requests for <see cref="Authority"/> to this server
    /// </summary>
    public string BackChannelHostOverride { get; set; } = "";
}
