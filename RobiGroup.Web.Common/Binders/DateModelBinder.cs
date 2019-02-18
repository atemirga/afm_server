using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace RobiGroup.Web.Common.Binders
{
    public class DateModelBinder : IModelBinder
    {
        private readonly IModelBinder baseBinder = new SimpleTypeModelBinder(typeof(DateTime));

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

                var valueAsString = valueProviderResult.FirstValue;

                //  valueAsString will have a string value of your date, e.g. '31/12/2017'
                if (!string.IsNullOrEmpty(valueAsString))
                {
                    var dateTime = DateTime.ParseExact(valueAsString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    bindingContext.Result = ModelBindingResult.Success(dateTime);
                }

                return Task.CompletedTask;
            }

            return baseBinder.BindModelAsync(bindingContext);
        }
    }
}