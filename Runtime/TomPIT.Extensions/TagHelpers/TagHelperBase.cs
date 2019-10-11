using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TomPIT.ComponentModel;
using TomPIT.Middleware;
using TomPIT.UI;

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
			return MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().ResolveMicroService(executingFilePath);
		}
	}
}
