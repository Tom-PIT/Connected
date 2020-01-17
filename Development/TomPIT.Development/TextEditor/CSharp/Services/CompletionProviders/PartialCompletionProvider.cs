using TomPIT.ComponentModel;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class PartialCompletionProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.Partial;
		protected override bool IncludeReferences => true;
	}
}