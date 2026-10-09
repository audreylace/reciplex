using System.Text.Json.Serialization;

namespace Reciplex.Server.Meilisearch.Responses;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MeilisearchTaskKind
{
    [JsonStringEnumMemberName("documentAdditionOrUpdate")]
    DocumentAdditionOrUpdate = 1,

    [JsonStringEnumMemberName("documentEdition")]
    DocumentEdition,

    [JsonStringEnumMemberName("documentDeletion")]
    DocumentDeletion,

    [JsonStringEnumMemberName("settingsUpdate")]
    SettingsUpdate,

    [JsonStringEnumMemberName("indexCreation")]
    IndexCreation,

    [JsonStringEnumMemberName("indexDeletion")]
    IndexDeletion,

    [JsonStringEnumMemberName("indexUpdate")]
    IndexUpdate,

    [JsonStringEnumMemberName("indexSwap")]
    IndexSwap,

    [JsonStringEnumMemberName("taskCancelation")]
    TaskCancellation,

    [JsonStringEnumMemberName("taskDeletion")]
    TaskDeletion,

    [JsonStringEnumMemberName("dumpCreation")]
    DumpCreation,

    [JsonStringEnumMemberName("snapshotCreation")]
    SnapshotCreation,

    [JsonStringEnumMemberName("export")]
    Export,

    [JsonStringEnumMemberName("upgradeDatabase")]
    UpgradeDatabase,

    [JsonStringEnumMemberName("indexCompaction")]
    IndexCompaction,

    [JsonStringEnumMemberName("networkTopologyChange")]
    NetworkTopologyChange,

    [JsonStringEnumMemberName("dsrUpdate")]
    DsrUpdate,

    [JsonStringEnumMemberName("dsrClear")]
    DsrClear,
}
