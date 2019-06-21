using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Models;
using TomPIT.Services;

namespace TomPIT.ComponentModel.UI
{
	public class ViewHelperArguments : DataModelContext
	{
		public ViewHelperArguments(IExecutionContext context, JObject e, RazorPage<IViewModel> view) : base(context)
		{
			Arguments = e;
			View = view;
		}

		public JObject Arguments { get; }
		public RazorPage<IViewModel> View { get; }
	}
}
