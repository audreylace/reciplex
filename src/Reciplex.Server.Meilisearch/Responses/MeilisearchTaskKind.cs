namespace Reciplex.Server.Meilisearch.Responses;

public enum MeilisearchTaskKind
{
    DocumentAdditionOrUpdate = 1,
    DocumentEdition,
    DocumentDeletion,
    SettingsUpdate,
    IndexCreation,
    IndexDeletion,
    IndexUpdate,
    IndexSwap,
    TaskCancelation,
    TaskDeletion,
    DumpCreation,
    SnapshotCreation,
    Export,
    UpgradeDatabase,
    IndexCompaction,
    NetworkTopologyChange,
    DsrUpdate,
    DsrClear,
}
