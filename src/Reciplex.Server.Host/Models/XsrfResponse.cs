namespace Reciplex.Server.Host.Models;

/// <summary>
/// Response sent from the xsf controller
/// </summary>
public class XsrfResponse
{
    /// <summary>
    /// The xsrf token the client will use for csrf protection
    /// </summary>
    public string XsrfToken { get; set; }
}
