using System.Collections.Generic;
using TomPIT.Ide.Analysis;
using TomPIT.Middleware;

namespace TomPIT.Development.Analysis.Providers
{
	internal class RouteKeysProvider : CodeAnalysisProvider
	{
		public RouteKeysProvider(IMiddlewareContext context) : base(context)
		{
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			return default;
			//var keys = context.Tenant.GetService<INavigationDesignService>().QueryRouteKeys(context.MicroService.Token);
			//var r = new List<ICodeAnalysisResult>();

			//foreach (var key in keys)
			//	r.Add(new CodeAnalysisResult { Text = key.RouteKey, Value = key.RouteKey, Description = key.Template });

			//return r;
		}
	}
}
