namespace Reciplex.Server.Database.SearchExporter;

public interface IRecipeMutationNotifyService
{
    void TriggerSearchExtraction();
    Task WaitForSearchExtractionTriggerAsync(TimeSpan timeout, CancellationToken ct);
    void TriggerSearchIndexDelete();
    Task WaitForSearchIndexDeleteTriggerAsync(TimeSpan timeout, CancellationToken ct);
    void TriggerRecipeDelete();
    Task WaitForRecipeDeleteTriggerAsync(TimeSpan timeout, CancellationToken ct);

    void TriggerBookDelete(); // todo-what calls this?
    Task WaitForBookDeleteTriggerAsync(TimeSpan timeout, CancellationToken ct);
}
