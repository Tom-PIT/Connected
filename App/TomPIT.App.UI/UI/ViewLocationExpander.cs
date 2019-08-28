using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;

namespace TomPIT.UI
{
	internal class ViewLocationExpander : IViewLocationExpander
	{
		public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
		{
			if (!context.ActionContext.ActionDescriptor.Properties.ContainsKey("viewKind"))
				return viewLocations;

			var viewKind = (ViewKind)context.ActionContext.ActionDescriptor.Properties["viewKind"];

			if(viewKind == ViewKind.Partial)
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
