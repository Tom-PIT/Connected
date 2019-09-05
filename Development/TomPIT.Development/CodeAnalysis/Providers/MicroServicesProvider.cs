using System.Collections.Generic;
using TomPIT.Analysis;
using TomPIT.Design;
using TomPIT.Design.Services;
using TomPIT.Services;

namespace TomPIT.Development.CodeAnalysis.Providers
{
	internal class MicroServicesProvider : CodeAnalysisProvider
	{
		public MicroServicesProvider(IExecutionContext context) : base(context)
		{
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IExecutionContext context, CodeAnalysisArgs e)
		{
			var references = context.Connection().GetService<IDiscoveryService>().FlattenReferences(context.MicroService.Token);

			var r = new List<ICodeAnalysisResult>
			{
				new CodeAnalysisResult { Text = context.MicroService.Name, Value = context.MicroService.Name }
			};

			foreach (var reference in references)
				r.Add(new CodeAnalysisResult { Text = reference.Name, Value = reference.Name });

			return r;
		}
	}
}
