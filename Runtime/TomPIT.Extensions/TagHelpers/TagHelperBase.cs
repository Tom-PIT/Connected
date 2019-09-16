using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TomPIT.ComponentModel;
using TomPIT.Middleware;

namespace TomPIT.TagHelpers
{
	public abstract class TagHelperBase : TagHelper
	{
		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		public string Name { get; set; }

		protected IMicroService ResolveMicroservice(string executingFilePath)
		{
			var ms = System.IO.Path.GetFileNameWithoutExtension(executingFilePath).Split(new char[] { '.' }, 2)[0];
			var ctx = ViewContext.ViewData.Model as IMiddlewareContext;

			return ctx.Tenant.GetService<IMicroServiceService>().Select(ms);
		}
	}
}
