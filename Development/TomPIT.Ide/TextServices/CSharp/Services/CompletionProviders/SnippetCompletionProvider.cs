using System.Collections.Generic;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders.Snippets;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders
{
	internal class SnippetCompletionProvider : CompletionProvider
	{
		private List<ISnippetProvider> _providers = null;

		public SnippetCompletionProvider()
		{
			Providers.Add(new DataCommandParametersSnippetProvider());
			//Providers.Add(new EntityImportSnippetProvider());
		}

		protected override List<ICompletionItem> OnProvideItems()
		{
			var result = new List<ICompletionItem>();

			foreach (var provider in Providers)
			{
				var results = provider.ProvideItems(Arguments);

				if (results != null && results.Count > 0)
					result.AddRange(results);
			}

			return result;
		}

		private List<ISnippetProvider> Providers
		{
			get
			{
				if (_providers == null)
					_providers = new List<ISnippetProvider>();

				return _providers;
			}
		}
	}
}
