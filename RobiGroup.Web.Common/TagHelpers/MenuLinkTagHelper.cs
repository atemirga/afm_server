using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RobiGroup.Web.Common.TagHelpers
{
    [HtmlTargetElement("menulink", Attributes = "controller-name, action-name, area-name, menu-text, menu-icon")]
    public class MenuLinkTagHelper : TagHelper
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string AreaName { get; set; }
        public string MenuText { get; set; }
        public string MenuIcon { get; set; }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public IUrlHelperFactory _UrlHelper { get; set; }

        public MenuLinkTagHelper(IUrlHelperFactory urlHelper)
        {
            _UrlHelper = urlHelper;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            StringBuilder sb = new StringBuilder();

            var urlHelper = _UrlHelper.GetUrlHelper(ViewContext);

            string menuUrl = urlHelper.Action(ActionName, ControllerName, new { area = AreaName});

            output.TagName = "li";

            var a = new TagBuilder("a");
            a.MergeAttribute("href", $"{menuUrl}");
            a.MergeAttribute("title", MenuText);

            if (!string.IsNullOrEmpty(MenuIcon))
            {
                if (MenuIcon.StartsWith("fa"))
                {
                    var i = new TagBuilder("i");
                    i.AddCssClass("fa");
                    i.AddCssClass(MenuIcon);
                    a.InnerHtml.AppendHtml(i);
                }
                else
                {
                    a.AddCssClass("bg-icon");
                    a.MergeAttribute("style", $"background-image: url('{MenuIcon}');");
                }
            }

            a.InnerHtml.Append(MenuText);

            var routeData = ViewContext.RouteData.Values;
            var currentController = routeData["controller"];
            var currentAction = routeData["action"];

            if (String.Equals(ActionName, currentAction as string, StringComparison.OrdinalIgnoreCase)
                && String.Equals(ControllerName, currentController as string, StringComparison.OrdinalIgnoreCase))
            {
                output.Attributes.Add("class", "active");
            }

            output.Content.AppendHtml(a);
        }
    }

    //[HtmlTargetElement("a")]
    //public class OrderByTagHelper : TagHelper
    //{
    //    public override void Process(TagHelperContext context, TagHelperOutput output)
    //    {
    //        var href = output.Attributes["href"];
    //        if (!href.Value.ToString().Contains("josephwoodward.co.uk"))
    //        {
    //           // output.Attributes["rel"] = "nofollow";
    //        }

    //        base.Process(context, output);
    //    }
    //}
}