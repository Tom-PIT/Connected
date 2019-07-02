using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class SubscriptionProvider : ComponentAnalysisProvider
	{
		public SubscriptionProvider(IExecutionContext context) : base(context)
		{

		}

		protected override string ComponentCategory => "Subscription";
		protected override bool FullyQualified => true;
	}
}
