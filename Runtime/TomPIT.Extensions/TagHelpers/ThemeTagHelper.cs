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
			//var etag = "0";

			if (Name.Contains("/"))
			{
				var tokens = Name.Split(new char[] { '/' }, 2);

				microService = tokens[0];
				name = tokens[1];

				var resolved = ResolveMicroservice(ViewContext.ExecutingFilePath);
				var ms = resolved == null ? null : ctx.Tenant.GetService<IMicroServiceService>().Select(resolved.Token);
				var reference = ctx.Tenant.GetService<IMicroServiceService>().Select(microService);

				if (reference == null)
					throw new RuntimeException(string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, microService));

				try
				{
					if (ms != null)
						ms.ValidateMicroServiceReference(reference.Name);
				}
				catch
				{
					ctx.MicroService.ValidateMicroServiceReference(reference.Name);
				}

				//etag = ctx.Tenant.GetService<IComponentService>().SelectComponent(reference.Token, ComponentCategories.Theme, name).Modified.Ticks.ToString();
			}
			else
			{
				var theme = ctx.Tenant.GetService<IComponentService>().SelectComponent(ctx.MicroService.Token, ComponentCategories.Theme, Name);

				if (theme == null)
					microService = ResolveMicroservice(ViewContext.ExecutingFilePath).Name;
				else
					microService = ctx.Tenant.GetService<IMicroServiceService>().Select(ctx.MicroService.Token).Name;
			}

			output.TagMode = TagMode.StartTagOnly;
			output.TagName = "link";

			output.Attributes.SetAttribute("rel", "stylesheet");
			output.Attributes.SetAttribute("type", "text/css");
			//output.Attributes.SetAttribute("href", $"{ctx.Services.Routing.RootUrl}/sys/themes/{microService}/{name}?{etag}");
			output.Attributes.SetAttribute("href", $"{ctx.Services.Routing.RootUrl}/sys/themes/{microService}/{name}");
		}
	}
}
