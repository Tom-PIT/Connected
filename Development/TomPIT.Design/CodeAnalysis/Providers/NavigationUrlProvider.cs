using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Design.Services;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class NavigationUrlProvider : CodeAnalysisProvider
	{
		public NavigationUrlProvider(IExecutionContext context) : base(context)
		{
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IExecutionContext context, CodeAnalysisArgs e)
		{
			var configs = context.Connection().GetService<IComponentService>().QueryConfigurations(context.MicroService.Token, "View");
			var r = new List<ICodeAnalysisResult>();

			foreach (var config in configs)
			{
				if (!(config is IView view) || string.IsNullOrWhiteSpace(view.Url))
					continue;

				r.Add(new CodeAnalysisResult(view.Url, view.Url, null));
			}

			return r;
		}
	}
}
