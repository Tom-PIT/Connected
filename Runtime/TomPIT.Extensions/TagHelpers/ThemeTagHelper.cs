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
			string microService;
			string name;
			var ctx = ViewContext.ViewData.Model as IMicroServiceContext;
			var tokens = Name.Split(new char[] { '/' }, 2);

			microService = tokens[0];
			name = tokens[1];

			ctx.MicroService.ValidateMicroServiceReference(ResolveMicroservice(ViewContext.ExecutingFilePath).Name);

			output.TagMode = TagMode.StartTagOnly;
			output.TagName = "link";

			output.Attributes.SetAttribute("rel", "stylesheet");
			output.Attributes.SetAttribute("type", "text/css");
			output.Attributes.SetAttribute("href", $"{ctx.Services.Routing.RootUrl}/sys/themes/{microService}/{name}");
		}
	}
}
