using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json.Linq;
using TomPIT.Services;

namespace TomPIT.ComponentModel.UI
{
	public class ViewHelperArguments : EventArguments
	{
		public ViewHelperArguments(IExecutionContext context, JObject e, RazorPage<IExecutionContext> view) : base(context)
		{
			Arguments = e;
			View = view;
		}

		public JObject Arguments { get; }
		public RazorPage<IExecutionContext> View { get; }
	}
}
