using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.TagHelpers
{
	public class ThemeTagHelper : TagHelperBase
	{
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			var microService = string.Empty;
			var name = Name;

			var ctx = ViewContext.ViewData.Model as IMicroServiceContext;

			if (Name.Contains("/"))
			{
				var tokens = Name.Split(new char[] { '/' }, 2);

				microService = tokens[0];
				name = tokens[1];

				var ms = ctx.Tenant.GetService<IMicroServiceService>().Select(ResolveMicroservice(ViewContext.ExecutingFilePath).Token);
				var reference = ctx.Tenant.GetService<IMicroServiceService>().Select(microService);

				if (reference == null)
					throw new RuntimeException(string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, microService));

				ms.ValidateMicroServiceReference(reference.Name);
			}
			else
			{
				var theme = ctx.Tenant.GetService<IComponentService>().SelectComponent(ctx.MicroService.Token, "Theme", Name);

				if (theme == null)
					microService = ResolveMicroservice(ViewContext.ExecutingFilePath).Name;
				else
					microService = ctx.Tenant.GetService<IMicroServiceService>().Select(ctx.MicroService.Token).Name;
			}

			output.TagMode = TagMode.StartTagOnly;
			output.TagName = "link";

			output.Attributes.SetAttribute("rel", "stylesheet");
			output.Attributes.SetAttribute("type", "text/css");
			output.Attributes.SetAttribute("href", string.Format("{0}/sys/themes/{1}/{2}", ctx.Services.Routing.RootUrl, microService, name));
		}
	}
}
