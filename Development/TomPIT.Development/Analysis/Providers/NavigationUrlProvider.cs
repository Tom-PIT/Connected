using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Ide.Analysis;
using TomPIT.Middleware;

namespace TomPIT.Development.Analysis.Providers
{
	internal class NavigationUrlProvider : CodeAnalysisProvider
	{
		public NavigationUrlProvider(IMiddlewareContext context) : base(context)
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

				r.Add(new CodeAnalysisResult(view.Url, view.Url, null));
			}

			return r;
		}
	}
}
