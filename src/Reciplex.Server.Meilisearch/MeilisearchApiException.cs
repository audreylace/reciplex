namespace Reciplex.Server.Meilisearch;

public class MeilisearchApiException(string message, Exception? ex = null)
    : Exception(message, ex) { }
