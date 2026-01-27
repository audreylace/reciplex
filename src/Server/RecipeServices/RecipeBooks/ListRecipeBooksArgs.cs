namespace Reciplex.Server.RecipeServices.RecipeBooks;

public class ListRecipeBooksArgs
{
    public string? BeforeBookId { get; set; }
    public string? AfterBookId { get; set; }
    public ListRecipeBooksOrdering? ResultOrder { get; set; }
    public required int ResultCount { get; set; }
}
