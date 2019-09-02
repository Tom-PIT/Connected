using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Design.Services;
using TomPIT.Development.Navigation;
using TomPIT.Services;

namespace TomPIT.Development.CodeAnalysis.Providers
{
	internal class RouteSiteMapsProvider : CodeAnalysisProvider
	{
		public RouteSiteMapsProvider(IExecutionContext context) : base(context)
		{
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IExecutionContext context, CodeAnalysisArgs e)
		{
			var keys = context.Connection().GetService<INavigationDesignService>().QuerySiteMapKeys(context.MicroService.Token);
			var r = new List<ICodeAnalysisResult>();

			foreach (var key in keys)
				r.Add(new CodeAnalysisResult { Text = key, Value = key });

			return r;
		}
	}
}
