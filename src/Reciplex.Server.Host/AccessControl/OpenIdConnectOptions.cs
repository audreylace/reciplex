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

#if DEBUG

    /// <summary>
    /// Disables HTTPs for communication with the OIDC server.
    /// Don't enable in production.
    /// </summary>
    public bool InsecureDisableHttps { get; set; }
#endif
}
