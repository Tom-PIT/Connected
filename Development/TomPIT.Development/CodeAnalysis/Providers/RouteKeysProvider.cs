using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Design;
using TomPIT.Design.Services;
using TomPIT.Development.Navigation;
using TomPIT.Navigation;
using TomPIT.Services;

namespace TomPIT.Development.CodeAnalysis.Providers
{
	internal class RouteKeysProvider : CodeAnalysisProvider
	{
		public RouteKeysProvider(IExecutionContext context) : base(context)
		{
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IExecutionContext context, CodeAnalysisArgs e)
		{
			var keys = context.Connection().GetService<INavigationDesignService>().QueryRouteKeys(context.MicroService.Token);
			var r = new List<ICodeAnalysisResult>();

			foreach (var key in keys)
				r.Add(new CodeAnalysisResult { Text = key.RouteKey, Value = key.RouteKey, Description = key.Template });

			return r;
		}
	}
}
