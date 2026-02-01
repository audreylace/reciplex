using Reciplex.Server.RecipeServices.RecipeBooks.Models;

namespace Reciplex.Server.RecipeServices.RecipeBooks.Results.CreateRecipeBook;

public class CreateRecipeBookResult
{
    public CreateRecipeBookResultOutcome Outcome { get; private set; }
    public RecipeBookDao Book
    {
        get => field ?? throw new InvalidOperationException("not a success result");
        private set;
    }
    public Dictionary<string, string[]> ValidationErrors { get; private set; } = [];

    public CreateRecipeBookResult(RecipeBookDao book)
    {
        Book = book;
        Outcome = CreateRecipeBookResultOutcome.Success;
    }

    public CreateRecipeBookResult(Dictionary<string, string[]> errors)
    {
        ValidationErrors = errors;
        Outcome = CreateRecipeBookResultOutcome.ValidationErrors;
    }

    public CreateRecipeBookResult(CreateRecipeBookResultOutcome outcome)
    {
        if (CreateRecipeBookResultOutcome.Success == outcome)
        {
            throw new ArgumentException("use success constructor", nameof(outcome));
        }
        Outcome = outcome;
    }

    public void EnsureSuccess()
    {
        if (Outcome != CreateRecipeBookResultOutcome.Success)
        {
            throw new Exception($"Create recipe book operation failed with outcome {Outcome}");
        }
    }
}
