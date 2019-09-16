using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json.Linq;
using TomPIT.Middleware;
using TomPIT.Models;

namespace TomPIT.UI
{
	public class ViewHelperArguments : MiddlewareContext
	{
		public ViewHelperArguments(IMiddlewareContext context, JObject e, RazorPage<IViewModel> view) : base(context)
		{
			Arguments = e;
			View = view;
		}

		public JObject Arguments { get; }
		public RazorPage<IViewModel> View { get; }
	}
}
