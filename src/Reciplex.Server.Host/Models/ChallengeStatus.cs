namespace Reciplex.Server.Host.Models;

public class ChallengeJsonResponse
{
    /// <summary>
    /// When true, the remote needs to invoke the challenge endpoint
    /// and run the authentication flow to use the service.
    /// </summary>
    public bool ChallengeRequired { get; set; }
}
