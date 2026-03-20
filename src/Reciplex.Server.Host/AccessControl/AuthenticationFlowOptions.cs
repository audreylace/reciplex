using System.ComponentModel.DataAnnotations;

namespace Reciplex.Server.Host.AccessControl;

/// <summary>
/// Options for the authentication flow in the application
/// </summary>
public class AuthenticationFlowOptions
{
    /// <summary>
    /// Section path
    /// </summary>
    public const string SectionPath = "Reciplex:AuthenticationFlow";

    /// <summary>
    /// The front-end page to redirect on authentication success
    /// </summary>
    [Required]
    public string PostSignInPath { get; set; } = "";

    /// <summary>
    /// In debug builds setting this to true binds
    /// fake credentials to each request
    /// </summary>
    public bool EnableAuthMocking { get; set; }
}
