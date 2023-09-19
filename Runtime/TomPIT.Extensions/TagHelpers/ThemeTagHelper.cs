using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TomPIT.ComponentModel;
using TomPIT.Middleware;

namespace TomPIT.TagHelpers
{
    public class ThemeTagHelper : TagHelperBase
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return;

            var ctx = ViewContext.ViewData.Model as IMicroServiceContext;
            var tokens = Name.Split(new char[] { '/' }, 2);

            var microService = tokens[0];
            var name = tokens[1];

            output.TagMode = TagMode.StartTagOnly;
            output.TagName = "link";

            output.Attributes.SetAttribute("rel", "stylesheet");
            output.Attributes.SetAttribute("type", "text/css");
            output.Attributes.SetAttribute("href", $"{ctx.Services.Routing.RootUrl}/sys/themes/{microService}/{name}");
        }
    }
}
