namespace Reciplex.Server.RecipeServices.RecipeBooks;

public class ListRecipeBooksArgs
{
    public RecipeBookKey? BeforeBookId { get; set; }
    public RecipeBookKey? AfterBookId { get; set; }
    public ListRecipeBooksOrdering? ResultOrder { get; set; }
    public required int ResultCount { get; set; }
}
