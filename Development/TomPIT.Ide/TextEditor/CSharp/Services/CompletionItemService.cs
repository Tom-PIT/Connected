using System.Collections.Generic;
using TomPIT.Ide.TextEditor.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextEditor.Languages;
using TomPIT.Ide.TextEditor.Services;

namespace TomPIT.Ide.TextEditor.CSharp.Services
{
	internal class CompletionItemService : CSharpEditorService, ICompletionItemService
	{
		private List<ICompletionProvider> _providers = null;
		public CompletionItemService(CSharpEditor editor) : base(editor)
		{
		}

		public ICompletionList ProvideCompletionItems(IPosition position, ICompletionContext context)
		{
			var result = new Languages.CompletionList();

			var model = Editor.Document.GetSemanticModelAsync().Result;
			var args = new CompletionProviderArgs(Editor, context, model, position);

			foreach (var provider in Providers)
			{
				var results = provider.ProvideItems(args);

				if (results != null && results.Count > 0)
					result.Suggestions.AddRange(results);
			}

			return result;
		}

		private List<ICompletionProvider> Providers
		{
			get
			{
				if (_providers == null)
				{
					_providers = new List<ICompletionProvider>
					{
						new DefaultProvider(),
						new ScriptReferenceProvider(),
						new SnippetCompletionProvider()
					};
				}

				return _providers;
			}
		}

	}
}
