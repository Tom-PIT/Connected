using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;
using TomPIT.UI;

namespace TomPIT.App.UI
{
	internal class ViewLocationExpander : IViewLocationExpander
	{
		public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
		{
			if (!context.ActionContext.ActionDescriptor.Properties.ContainsKey("viewKind"))
				return viewLocations;

			var viewKind = (ViewKind)context.ActionContext.ActionDescriptor.Properties["viewKind"];

			if (viewKind == ViewKind.View)
			{
				var result = new List<string>
				{
					"/Views/Dynamic/View/{0}.cshtml"
				};

				foreach (var location in viewLocations)
					result.Add(location);

				return result;
			}
			if (viewKind == ViewKind.Partial)
			{
				var result = new List<string>
				{
					"/Views/Dynamic/Partial/{0}.cshtml",
					"/Views/Dynamic/Partial/{0}/{1}.cshtml"
				};

				foreach (var location in viewLocations)
					result.Add(location);

				return result;
			}

			return viewLocations;
		}

		public void PopulateValues(ViewLocationExpanderContext context)
		{
		}
	}
}
