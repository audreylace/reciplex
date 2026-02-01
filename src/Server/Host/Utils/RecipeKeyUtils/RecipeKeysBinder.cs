using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reciplex.Server.RecipeServices.Recipes;

namespace Reciplex.Server.Host.Utils.RecipeKeyUtils;

/// <summary>
/// Binds <see cref="RecipeKey"/> using <see cref="IStringRecipeKeyInterop"/> from strings
/// </summary>
public class RecipeKeysBinder : IModelBinder, IModelBinderProvider
{
    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        string modelName = bindingContext.ModelName;
        // Try to fetch the value of the argument by name
        ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }
        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
        string? value = valueProviderResult.FirstValue;

        // Check if the argument value is null or empty
        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        var stringIdInterop =
            bindingContext.HttpContext.RequestServices.GetRequiredService<IStringRecipeKeyInterop>();
        RecipeKey? recipeKey = stringIdInterop.AsKey(value);
        if (recipeKey is null)
        {
            // Non-integer arguments result in model state errors
            bindingContext.ModelState.TryAddModelError(modelName, "not a valid recipe id");
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(
            bindingContext.ModelType == typeof(RecipeKey) ? recipeKey.Value : recipeKey
        );
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        Type targetType = context.Metadata.ModelType;
        if (targetType != typeof(RecipeKey) && targetType != typeof(RecipeKey?))
        {
            return null;
        }

        return this;
    }
}
