namespace Reciplex.Server.Database.RecipeBooksDomain;

public class ListRecipeBooksArgs
{
    public string? BeforeBookId { get; set; }
    public string? AfterBookId { get; set; }
    public RecordOrdering? ResultOrder { get; set; }
    public required int ResultCount { get; set; }
}
