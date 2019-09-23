using System.Collections.Generic;
using TomPIT.Ide.TextEditor.Languages;

namespace TomPIT.Ide.TextEditor.CSharp.Services.CompletionProviders.Snippets
{
	internal interface ISnippetProvider
	{
		List<ICompletionItem> ProvideItems(CompletionProviderArgs e);
	}
}
