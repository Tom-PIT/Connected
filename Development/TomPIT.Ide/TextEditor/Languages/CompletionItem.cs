using System.Collections.Generic;

namespace TomPIT.Ide.TextEditor.Languages
{
	public class CompletionItem : ICompletionItem
	{
		private List<ISingleEditOperation> _operations = null;
		private List<string> _commitCharacters = null;
		private List<CompletionItemTag> _tags = null;

		public List<ISingleEditOperation> AdditionalTextEdits
		{
			get
			{
				if (_operations == null)
					_operations = new List<ISingleEditOperation>();

				return _operations;
			}
		}

		public ICommand Command { get; set; }

		public List<string> CommitCharacters
		{
			get
			{
				if (_commitCharacters == null)
					_commitCharacters = new List<string>();

				return _commitCharacters;
			}
		}

		public string Detail { get; set; }

		public string Documentation { get; set; }

		public string FilterText { get; set; }

		public string InsertText { get; set; }

		public CompletionItemInsertTextRule InsertTextRules { get; set; }

		public CompletionItemKind Kind { get; set; }

		public string Label { get; set; }

		public bool Preselect { get; set; }

		public IRange Range { get; set; }
		public string SortText { get; set; }

		public List<CompletionItemTag> Tags
		{
			get
			{
				if (_tags == null)
					_tags = new List<CompletionItemTag>();

				return _tags;
			}
		}

		public static bool SupportsDescription(CompletionItemKind kind)
		{
			return kind == CompletionItemKind.Constant
				|| kind == CompletionItemKind.Field
				|| kind == CompletionItemKind.Keyword
				|| kind == CompletionItemKind.Property
				|| kind == CompletionItemKind.Reference
				|| kind == CompletionItemKind.Snippet
				|| kind == CompletionItemKind.Text
				|| kind == CompletionItemKind.Value
				|| kind == CompletionItemKind.Variable;
		}
	}
}