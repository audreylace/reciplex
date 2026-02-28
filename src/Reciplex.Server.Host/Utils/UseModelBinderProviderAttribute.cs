using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Reciplex.Server.Host.Utils;

/// <summary>
/// Configures a route input to use application configured binders
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
public class UseModelBinderProviderAttribute : Attribute, IBindingSourceMetadata
{
    public BindingSource BindingSource { get; } = BindingSource.Special;
}
