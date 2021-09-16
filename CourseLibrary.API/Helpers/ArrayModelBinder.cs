using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CourseLibrary.API.Helpers
{
    public class ArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            /*
             * Our binder only works only on enumerable types
             */
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            /*
             * If it is enumerable type, get the inputted value through the value provider
             */
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();

            /*
             * If the value is null, we check for that and return it as null, so that
             * it allows us to check that in our method and we can return a bad request
             */
            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            /*
            * If the value is not null or whitespace, and
            * the type of the model is enumerable,
            * get the enumerable type and a converter
            */
            var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            var converter = TypeDescriptor.GetConverter(elementType);

            /*
             * Convert each item in the value list to the enumerable type
             */
            var values = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => converter.ConvertFromString(x.Trim())).ToArray();

            /*
             * Create an array of that type and set as Model value
             */
            var typedValues = Array.CreateInstance(elementType, values.Length);
            values.CopyTo(typedValues, 0);
            bindingContext.Model = typedValues;

            /*
             * Return a successful result, passing in the Model 
             */
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;
        }
    }
}
