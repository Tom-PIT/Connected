using System.Collections.Generic;
using System.Linq;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Ide.TextServices.Razor.Services.ActionProviders;
using TomPIT.Ide.TextServices.Services;

namespace TomPIT.Ide.TextServices.Razor.Services
{
	internal class CodeActionService : RazorEditorService, ICodeActionService
	{
		private List<IActionProvider> _providers = null;
		public CodeActionService(RazorEditor editor) : base(editor)
		{
		}

		public List<ICodeAction> ProvideCodeActions(IRange range, ICodeActionContext context)
		{
			var result = new List<ICodeAction>();
			var caret = Editor.GetMappedCaret(range);

			if (caret == -1)
				return null;

			var model = Editor.SemanticModel;

			if (model == null)
				return result;

			var token = model.SyntaxTree.GetRoot().FindToken(caret);

			if (token.Parent == null)
				return null;

			var args = new CodeActionProviderArgs(Editor, context, model, token.Parent);

			foreach (var provider in Providers)
			{
				var results = provider.GetActions(args);

				if (results != null && results.Count > 0)
					result.AddRange(results);
			}

			return result.GroupBy(f => f.Title).Select(f => f.First()).ToList();
		}

		private List<IActionProvider> Providers
		{
			get
			{
				if (_providers == null)
				{
					_providers = new List<IActionProvider>
					{
						new UsingActionProvider()
					};
				}

				return _providers;
			}
		}
	}
}
