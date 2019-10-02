using TomPIT.ComponentModel;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class StringTableProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.StringTable;
		protected override bool IncludeReferences => true;
	}
}
