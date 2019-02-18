using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Localization;

namespace RobiGroup.Web.Common.Localizer
{
    public class ApplicationDisplayMetadataProvider<TResourceType> : IDisplayMetadataProvider
    {
        private readonly IStringLocalizerFactory _stringLocalizerFactory;

        public ApplicationDisplayMetadataProvider(IStringLocalizerFactory stringLocalizerFactory)
        {
            _stringLocalizerFactory = stringLocalizerFactory;
        }

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;
            
            if (IsTransformRequired(propertyName, modelMetadata, propertyAttributes))
            {
                var localizer = _stringLocalizerFactory.Create(typeof(TResourceType));
                modelMetadata.DisplayName = () => localizer[propertyName] ?? propertyName;
            }
            else if (modelMetadata.IsEnum)
            {
                foreach (var displayNamesAndValue in modelMetadata.EnumGroupedDisplayNamesAndValues)
                {
                   // displayNamesAndValue.Key
                }
            }
        } 

        private static bool IsTransformRequired(string propertyName, DisplayMetadata modelMetadata, IReadOnlyList<object> propertyAttributes)
        {
            if (!string.IsNullOrEmpty(modelMetadata.SimpleDisplayProperty))
                return false;

            if (propertyAttributes.OfType<DisplayNameAttribute>().Any())
                return false;

            if (propertyAttributes.OfType<DisplayAttribute>().Any())
                return false;

            if (string.IsNullOrEmpty(propertyName))
                return false;

            return true;
        }
    }
}
