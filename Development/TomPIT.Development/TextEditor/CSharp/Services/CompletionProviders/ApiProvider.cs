using TomPIT.ComponentModel;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class ApiProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.Api;
		protected override bool IncludeReferences => true;
	}
}
