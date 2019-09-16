using TomPIT.Middleware;

namespace TomPIT.Development.Analysis.Providers
{
	internal class BigDataPartitionProvider : ComponentAnalysisProvider
	{
		public BigDataPartitionProvider(IMiddlewareContext context) : base(context)
		{

		}

		protected override string ComponentCategory => "BigDataPartition";
		protected override bool FullyQualified => true;
		protected override bool IncludeReferences => true;
	}
}
