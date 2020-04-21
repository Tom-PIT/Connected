using TomPIT.ComponentModel;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class BundleCompletionProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.ScriptBundle;
		protected override bool IncludeReferences => true;
	}
}