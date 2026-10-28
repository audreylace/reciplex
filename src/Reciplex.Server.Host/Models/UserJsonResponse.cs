using System.Diagnostics.CodeAnalysis;
using Reciplex.Server.Database.UsersDomain;

namespace Reciplex.Server.Host.Models;

public class UserJsonResponse
{
    public required string DisplayName { get; init; }
    public required string UserKey { get; init; }
    public required string ConcurrencyTag { get; init; }
}
