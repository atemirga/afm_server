using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace RobiGroup.Web.Common
{
    public static class HttpRequestExtensions
    {
        private const string RequestedWithHeader = "X-Requested-With";
        private const string XmlHttpRequest = "XMLHttpRequest";

        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.Headers != null)
            {
                return request.Headers[RequestedWithHeader] == XmlHttpRequest;
            }

            return false;
        }

        public static IDictionary<string, object> ToRouteDataValues(this IQueryCollection queryString, object additionalValues = null)
        {
            var dictionary = queryString.ToDictionary(d => d.Key, d => d.Value as object);

            if (additionalValues != null)
            {
                foreach (var property in additionalValues.GetType().GetProperties())
                {
                    dictionary.Add(property.Name, property.GetValue(additionalValues, null));
                }
            }

            return dictionary;
        }
    }
}