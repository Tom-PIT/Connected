using TomPIT.ComponentModel;
using TomPIT.Middleware;

namespace TomPIT.Development.Analysis.Providers
{
	internal class SubscriptionProvider : ComponentAnalysisProvider
	{
		public SubscriptionProvider(IMiddlewareContext context) : base(context)
		{

		}

		protected override string ComponentCategory => ComponentCategories.Subscription;
		protected override bool FullyQualified => true;
	}
}
