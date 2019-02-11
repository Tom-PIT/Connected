using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.TagHelpers
{
	public class BundleTagHelper : TagHelperBase
	{
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			var microService = string.Empty;
			var name = Name;
			var ctx = ViewContext.ViewData.Model as IExecutionContext;

			if (Name.Contains("/"))
			{
				var tokens = Name.Split(new char[] { '/' }, 2);

				microService = tokens[0];
				name = tokens[1];

				var ms = ctx.Connection().GetService<IMicroServiceService>().Select(ctx.MicroService.Token);
				var reference = ctx.Connection().GetService<IMicroServiceService>().Select(microService);

				if (reference == null)
					throw new RuntimeException(string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, microService)).WithMetrics(ctx);

				ms.ValidateMicroServiceReference(ctx.Connection(), reference.Name);
			}
			else
			{
				var bundle = ctx.Connection().GetService<IComponentService>().SelectComponent(ctx.MicroService.Token, "Bundle", Name);

				if (bundle == null)
					microService = ResolveMicroservice(ViewContext.ExecutingFilePath).Name;
				else
					microService = ctx.Connection().GetService<IMicroServiceService>().Select(ctx.MicroService.Token).Name;
			}

			output.TagName = "script";
			output.Attributes.SetAttribute("src", string.Format("{0}/sys/bundles/{1}/{2}", ctx.RootUrl(), microService, name));
		}
	}
}
