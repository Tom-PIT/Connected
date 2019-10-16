using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Ide.TextServices.Razor.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Services;

namespace TomPIT.Ide.TextServices.Razor.Services
{
	internal class CompletionItemService : RazorEditorService, ICompletionItemService
	{
		private List<ICompletionProvider> _providers = null;
		public CompletionItemService(RazorEditor editor) : base(editor)
		{
		}

		public ICompletionList ProvideCompletionItems(IPosition position, ICompletionContext context)
		{
			var result = new Languages.CompletionList();

			var model = Editor.SemanticModel;

			if (model == null)
				return result;

			var args = new CompletionProviderArgs(Editor, context, model, position);

			foreach (var provider in Providers)
			{
				if (!provider.WillProvideItems(args, ImmutableArray.Create(result.Suggestions.ToArray())))
					continue;

				var results = provider.ProvideItems(args);

				if (results != null && results.Count > 0)
					result.Suggestions.AddRange(results);
			}

			return result.Suggestions.Count == 0 ? null : result;
		}

		private List<ICompletionProvider> Providers
		{
			get
			{
				if (_providers == null)
				{
					_providers = new List<ICompletionProvider>
					{
						new HtmlAttributeItemsProvider(),
						new SnippetCompletionProvider(),
						new CompletionProviders.AttributeCompletionProvider(),
						new CompletionProviders.DefaultProvider(),
						new CompletionProviders.InteropProvider()
					};
				}

				return _providers;
			}
		}

	}
}
