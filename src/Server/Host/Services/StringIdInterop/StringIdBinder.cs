using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Reciplex.Server.Host.Services.StringIdInterop;

public class StringIdBinder : IModelBinder
{
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

        var encoder =
            bindingContext.HttpContext.RequestServices.GetRequiredService<IStringIdInterop>();
        long? id = encoder.AsLong(value);
        if (id is null)
        {
            // Non-integer arguments result in model state errors
            bindingContext.ModelState.TryAddModelError(modelName, "not a valid value");
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(id);
        return Task.CompletedTask;
    }
}
