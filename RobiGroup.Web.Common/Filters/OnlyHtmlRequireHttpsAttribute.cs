using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RobiGroup.Web.Common.Filters
{
    public class OnlyHtmlRequireHttpsAttribute : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext.HttpContext.Request.Path.HasValue 
                && !filterContext.HttpContext.Request.Host.Host.Contains("localhost")
                && !filterContext.HttpContext.Request.Path.StartsWithSegments("/api")
                && !filterContext.HttpContext.Request.Path.StartsWithSegments("/token"))
            {
                base.OnAuthorization(filterContext);
            }
        }
    }
}