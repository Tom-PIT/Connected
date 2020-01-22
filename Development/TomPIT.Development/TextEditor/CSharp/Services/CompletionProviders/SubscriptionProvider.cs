using TomPIT.ComponentModel;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class SubscriptionProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.Subscription;
	}
}
