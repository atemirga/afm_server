using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace RobiGroup.Web.Common.Localizer
{
    public static class LocalizationExtensions
    {
        public static IDictionary<string, string> ToDictionary<T>(this IStringLocalizer<T> localizer, params string[] names)
        {
            var localizedDictionary = new Dictionary<string, string>();

            foreach (var s in names)
            {
                localizedDictionary[s] = localizer[s];
            }

            return localizedDictionary;
        }
    }
}