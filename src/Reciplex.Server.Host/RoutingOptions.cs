using System.ComponentModel.DataAnnotations;

namespace Reciplex.Server.Host;

/// <summary>
/// Settings for routing
/// </summary>
public class RoutingOptions
{
    /// <summary>
    /// Section path
    /// </summary>
    public const string SectionPath = "Reciplex:Routing";

    /// <summary>
    /// The domain the server is hosted on
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Domain { get; set; } = "";
}
