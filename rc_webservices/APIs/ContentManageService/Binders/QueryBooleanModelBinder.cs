using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ContentManageService.Binders
{
    /// <summary>
    /// read: https://thomaslevesque.com/2020/01/30/handling-query-string-parameters-with-no-value-in-asp-net-core/
    /// https://stackoverflow.com/questions/38551698/gettype-on-nullable-boolean
    /// </summary>
    public class QueryBooleanModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (result == ValueProviderResult.None)
            {
                // Parameter is missing, interpret as false
                bindingContext.Result = ModelBindingResult.Success(false);
            }
            else
            {
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, result);
                var rawValue = result.FirstValue;
                if (string.IsNullOrEmpty(rawValue))
                {
                    // Value is empty, interpret as true
                    bindingContext.Result = ModelBindingResult.Success(null);
                }
                else if (bool.TryParse(rawValue, out var boolValue))
                {
                    // Value is a valid boolean, use that value
                    bindingContext.Result = ModelBindingResult.Success(boolValue);
                }
                else
                {
                    // Value is something else, fail
                    bindingContext.ModelState.TryAddModelError(
                        bindingContext.ModelName,
                        "Value must be false, true, or empty.");
                }
            }

            return Task.CompletedTask;
        }
    }

    public class QueryBooleanModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(bool?) &&
                context.BindingInfo.BindingSource != null &&
                context.BindingInfo.BindingSource.CanAcceptDataFrom(BindingSource.Query))
            {
                return new QueryBooleanModelBinder();
            }

            return null;
        }
    }
}
