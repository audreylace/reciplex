using Reciplex.Server.Database.SearchIndexDomain;

namespace Reciplex.Server.SearchExporter;

/// <summary>
/// The search id pointer
/// </summary>
/// <param name="Kind">the id kind</param>
/// <param name="Id">the record id</param>
public record class SearchIdPointer(RecipeBookSearchIndexDocumentKind Kind, long Id);
