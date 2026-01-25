namespace Recipe.Database.Queries;

public class RecipeBookListQueryArgs
{
    public int? PageSize { get; set; }
    public long? BeforeId { get; set; }
    public long? AfterId { get; set; }
    public ResultOrdering? Order { get; set; }
}
