using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Ide.Analysis;
using TomPIT.Middleware;

namespace TomPIT.Development.Analysis.Providers
{
	internal class NavigationViewUrlProvider : CodeAnalysisProvider
	{
		public NavigationViewUrlProvider(IMiddlewareContext context) : base(context)
		{
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			var configs = context.Tenant.GetService<IComponentService>().QueryConfigurations(context.MicroService.Token, ComponentCategories.View);
			var r = new List<ICodeAnalysisResult>();

			foreach (var config in configs)
			{
				if (!(config is IViewConfiguration view) || string.IsNullOrWhiteSpace(view.Url))
					continue;

				var viewName = view.ComponentName();

				r.Add(new CodeAnalysisResult(viewName, $"{context.MicroService.Name}/{viewName}", view.Url));
			}

			return r;
		}
	}
}
