using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json.Linq;
using TomPIT.Runtime;

namespace TomPIT.ComponentModel.Views
{
	public class ViewHelperArguments : EventArguments
	{
		public ViewHelperArguments(IApplicationContext model, JObject e, RazorPage<IApplicationContext> view) : base(model)
		{
			Arguments = e;
			View = view;
		}

		public JObject Arguments { get; }
		public RazorPage<IApplicationContext> View { get; }
	}
}
