using TomPIT.ComponentModel;
using TomPIT.Middleware;

namespace TomPIT.Development.Analysis.Providers
{
	internal class QueueWorkerProvider : ComponentAnalysisProvider
	{
		public QueueWorkerProvider(IMiddlewareContext context) : base(context)
		{

		}

		protected override string ComponentCategory => ComponentCategories.Queue;
		protected override bool FullyQualified => true;
	}
}
