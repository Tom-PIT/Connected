using System.Collections.Generic;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders.Snippets
{
	internal class SnippetProvider : ISnippetProvider
	{
		public List<ICompletionItem> ProvideItems(CompletionProviderArgs e)
		{
			Arguments = e;

			return OnProvideItems();
		}

		protected virtual List<ICompletionItem> OnProvideItems()
		{
			return null;
		}

		protected CompletionProviderArgs Arguments { get; private set; }
		protected CSharpEditorBase Editor => Arguments.Editor as CSharpEditorBase;
	}
}
