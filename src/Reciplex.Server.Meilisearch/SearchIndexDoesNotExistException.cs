namespace Reciplex.Server.Meilisearch;

public class SearchIndexDoesNotExistException(string indexName)
    : MeilisearchApiException($"index with name {indexName} does not exist", null) { }
