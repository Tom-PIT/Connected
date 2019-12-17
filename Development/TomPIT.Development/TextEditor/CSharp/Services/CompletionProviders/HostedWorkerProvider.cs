using TomPIT.ComponentModel;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class HostedWorkerProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.HostedWorker;
		protected override bool IncludeReferences => true;
	}
}
