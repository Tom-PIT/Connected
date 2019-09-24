using System.Collections.Generic;
using TomPIT.Ide.Analysis;
using TomPIT.Middleware;

namespace TomPIT.Development.Analysis.Providers
{
	internal class MicroServicesProvider : CodeAnalysisProvider
	{
		public MicroServicesProvider(IMiddlewareContext context) : base(context)
		{
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			return default;
			//var references = context.Tenant.GetService<IDiscoveryService>().FlattenReferences(context.MicroService.Token);

			//var r = new List<ICodeAnalysisResult>
			//{
			//	new CodeAnalysisResult { Text = context.MicroService.Name, Value = context.MicroService.Name }
			//};

			//foreach (var reference in references)
			//	r.Add(new CodeAnalysisResult { Text = reference.Name, Value = reference.Name });

			//return r;
		}
	}
}
