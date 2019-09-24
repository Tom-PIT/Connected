using System.Collections.Generic;
using TomPIT.Ide.Analysis;
using TomPIT.Middleware;

namespace TomPIT.Development.Analysis.Providers
{
	internal class RouteSiteMapsProvider : CodeAnalysisProvider
	{
		public RouteSiteMapsProvider(IMiddlewareContext context) : base(context)
		{
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			return default;
			//var keys = context.Tenant.GetService<INavigationDesignService>().QuerySiteMapKeys(context.MicroService.Token);
			//var r = new List<ICodeAnalysisResult>();

			//foreach (var key in keys)
			//	r.Add(new CodeAnalysisResult { Text = key, Value = key });

			//return r;
		}
	}
}
