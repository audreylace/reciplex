namespace Reciplex.Server.Database.SearchExtractionWorker;

internal interface IDurableChangeQueueEntryPostedBroadcaster
{
    void BroadcastEntryAdded(long id);
}
