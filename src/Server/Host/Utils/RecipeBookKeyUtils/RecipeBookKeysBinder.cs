using Microsoft.AspNetCore.Mvc.ModelBinding;
using Reciplex.Server.RecipeServices.RecipeBooks;

namespace Reciplex.Server.Host.Utils.RecipeBookKeyUtils;

/// <summary>
/// Binds <see cref="RecipeBookKey"/> using <see cref="IStringRecipeBookKeyInterop"/> from strings
/// </summary>
public class RecipeBookKeysBinder : IModelBinder, IModelBinderProvider
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
            bindingContext.HttpContext.RequestServices.GetRequiredService<IStringRecipeBookKeyInterop>();
        RecipeBookKey? recipeKey = stringIdInterop.AsKey(value);
        if (recipeKey is null)
        {
            // Non-integer arguments result in model state errors
            bindingContext.ModelState.TryAddModelError(modelName, "not a valid recipe book id");
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(
            bindingContext.ModelType == typeof(RecipeBookKey) ? recipeKey.Value : recipeKey
        );
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        Type targetType = context.Metadata.ModelType;
        if (targetType != typeof(RecipeBookKey) && targetType != typeof(RecipeBookKey?))
        {
            return null;
        }

        return this;
    }
}
