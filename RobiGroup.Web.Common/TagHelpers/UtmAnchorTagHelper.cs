using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RobiGroup.Web.Common.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "asp-utm")]
    public class UtmAnchorTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        [HtmlAttributeName("asp-utm")]
        public bool Utm { get; set; }

        public override int Order
        {
            get { return int.MaxValue; }
        }


        public UtmAnchorTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Utm && _httpContextAccessor.HttpContext.Request.QueryString.HasValue)
            {
                var hrefAttr = output.Attributes["href"];
                string href = hrefAttr.Value?.ToString();
                if (href != null)
                {
                    href += (href.Contains("?") ? "&" : "?") + _httpContextAccessor.HttpContext.Request.QueryString.Value.Remove(0, 1);
                    output.Attributes.RemoveAll("href");
                    output.Attributes.Add("href", href);
                }

            }

            base.Process(context, output);
        }
    }
}