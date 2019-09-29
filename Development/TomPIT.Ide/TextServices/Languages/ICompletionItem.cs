using System.Collections.Generic;

namespace TomPIT.Ide.TextServices.Languages
{
	public enum CompletionItemTag
	{
		Deprecated = 1
	}
	public enum CompletionItemInsertTextRule
	{
		KeepWhitespace = 1,
		InsertAsSnippet = 4
	}

	public enum CompletionItemKind
	{
		Method = 0,
		Function = 1,
		Constructor = 2,
		Field = 3,
		Variable = 4,
		Class = 5,
		Struct = 6,
		Interface = 7,
		Module = 8,
		Property = 9,
		Event = 10,
		Operator = 11,
		Unit = 12,
		Value = 13,
		Constant = 14,
		Enum = 15,
		EnumMember = 16,
		Keyword = 17,
		Text = 18,
		Color = 19,
		File = 20,
		Reference = 21,
		CustomColor = 22,
		Folder = 23,
		TypeParameter = 24,
		Snippet = 25
	}
	public interface ICompletionItem
	{
		List<ISingleEditOperation> AdditionalTextEdits { get; }
		ICommand Command { get; set; }
		List<string> CommitCharacters { get; }
		string Detail { get; }
		string Documentation { get; }
		string FilterText { get; }
		string InsertText { get; }
		CompletionItemInsertTextRule InsertTextRules { get; }
		CompletionItemKind Kind { get; }
		string Label { get; }
		bool Preselect { get; }
		IRange Range { get; set; }
		string SortText { get; set; }
		List<CompletionItemTag> Tags { get; }
	}
}
