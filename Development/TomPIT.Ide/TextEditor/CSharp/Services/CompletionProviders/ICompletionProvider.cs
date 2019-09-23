using System.Collections.Generic;
using TomPIT.Ide.TextEditor.Languages;

namespace TomPIT.Ide.TextEditor.CSharp.Services.CompletionProviders
{
	internal interface ICompletionProvider
	{
		List<ICompletionItem> ProvideItems(CompletionProviderArgs e);
	}
}
