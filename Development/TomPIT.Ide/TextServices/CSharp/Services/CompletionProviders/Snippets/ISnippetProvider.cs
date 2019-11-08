using System.Collections.Generic;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders.Snippets
{
	internal interface ISnippetProvider
	{
		List<ICompletionItem> ProvideItems(CompletionProviderArgs e);
	}
}
