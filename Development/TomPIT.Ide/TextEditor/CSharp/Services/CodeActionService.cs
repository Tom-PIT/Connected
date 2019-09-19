using System.Collections.Generic;
using TomPIT.Ide.TextEditor.CSharp.Services.ActionProviders;
using TomPIT.Ide.TextEditor.Languages;
using TomPIT.Ide.TextEditor.Services;

namespace TomPIT.Ide.TextEditor.CSharp.Services
{
	internal class CodeActionService : CSharpEditorService, ICodeActionService
	{
		private List<IActionProvider> _providers = null;
		public CodeActionService(CSharpEditor editor) : base(editor)
		{
		}

		public List<ICodeAction> ProvideCodeActions(IRange range, ICodeActionContext context)
		{
			var result = new List<ICodeAction>();

			var span = Editor.Document.GetSpan(range);
			var model = Editor.Document.GetSemanticModelAsync().Result;
			var node = model.SyntaxTree.GetRoot().FindNode(span);
			var args = new CodeActionProviderArgs(Editor, context, model, node);

			foreach (var provider in Providers)
			{
				var results = provider.GetActions(args);

				if (results != null && results.Count > 0)
					result.AddRange(results);
			}

			return result;
		}

		private List<IActionProvider> Providers
		{
			get
			{
				if (_providers == null)
				{
					_providers = new List<IActionProvider>();

					_providers.Add(new UsingActionProvider());
				}

				return _providers;
			}
		}
	}
}
