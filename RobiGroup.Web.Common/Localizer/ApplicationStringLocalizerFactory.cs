using System;
using System.Reflection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RobiGroup.Web.Common.Localizer
{
    public class ApplicationStringLocalizerFactory<TResourceType> : ResourceManagerStringLocalizerFactory
    {
        public ApplicationStringLocalizerFactory(IOptions<LocalizationOptions> localizationOptions, ILoggerFactory loggerFactory) : base(localizationOptions, loggerFactory)
        {
        }

        protected override ResourceManagerStringLocalizer CreateResourceManagerStringLocalizer(Assembly assembly, string baseName)
        {
            var type = Type.GetType(baseName);
            if (type != null && type.GetTypeInfo().IsEnum)
            {
                baseName = typeof(TResourceType).FullName;
            }

            return base.CreateResourceManagerStringLocalizer(assembly, baseName); ;
        }
    }
}