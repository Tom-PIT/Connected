using TomPIT.ComponentModel;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class ConnectionCompletionProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.Connection;
	}
}
