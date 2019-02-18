using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RobiGroup.Web.Common
{
    public static class PageHelpers
    {
        public static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                             ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }

        public static string PageNavTreeClass(this IUrlHelper url, string pageTree)
        {
            return url.Page("").StartsWith(pageTree, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}