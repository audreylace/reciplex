namespace Reciplex.Server.Database.SearchExporter;

interface IRecipeIndexCreationCoordinator
{
    Task DeclareIndexSetupAsync();
    Task<bool> WaitForIndexSetupAsync(CancellationToken ct);
}
