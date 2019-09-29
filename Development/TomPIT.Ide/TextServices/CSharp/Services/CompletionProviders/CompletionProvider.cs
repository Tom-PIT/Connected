using System.Collections.Generic;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders
{
	public class CompletionProvider : ICompletionProvider
	{
		public List<ICompletionItem> ProvideItems(CompletionProviderArgs e)
		{
			Arguments = e;

			return OnProvideItems();
		}

		public virtual bool WillProvideItems(CompletionProviderArgs e, IReadOnlyCollection<ICompletionItem> existing)
		{
			return true;
		}
		protected CompletionProviderArgs Arguments { get; private set; }
		public CSharpEditorBase Editor => Arguments.Editor as CSharpEditorBase;

		protected virtual List<ICompletionItem> OnProvideItems()
		{
			return null;
		}

		protected (ICompletionItem, bool) FromCompletion(Microsoft.CodeAnalysis.Completion.CompletionService service, Microsoft.CodeAnalysis.Completion.CompletionItem item, bool resolveDescription)
		{
			var descriptionResolved = false;

			var result = new CompletionItem
			{
				FilterText = item.FilterText,
				InsertText = item.DisplayText,
				Kind = ResolveKind(item),
				Label = item.DisplayText,
				SortText = item.SortText
			};

			if (resolveDescription && CompletionItem.SupportsDescription(result.Kind))
			{
				descriptionResolved = true;
				var description = service.GetDescriptionAsync(Editor.Document, item).Result;

				if (description != null)
					result.Detail = description.Text;
			}

			foreach (var rule in item.Rules.CommitCharacterRules)
			{
				foreach (var character in rule.Characters)
					result.CommitCharacters.Add(character.ToString());
			}

			return (result, descriptionResolved);
		}

		protected static CompletionItemKind ResolveKind(Microsoft.CodeAnalysis.Completion.CompletionItem item)
		{
			if (item.Tags == null || item.Tags.Length == 0)
				return CompletionItemKind.Text;

			var value = item.Tags[0];

			if (string.Compare("Variable", value, true) == 0)
				return CompletionItemKind.Variable;
			else if (string.Compare("Value", value, true) == 0)
				return CompletionItemKind.Value;//boolean
			else if (string.Compare("Class", value, true) == 0)
				return CompletionItemKind.Class;
			else if (string.Compare("Constant", value, true) == 0)
				return CompletionItemKind.Constant;
			else if (string.Compare("Constructor", value, true) == 0)
				return CompletionItemKind.Constructor;
			else if (string.Compare("Enum", value, true) == 0)
				return CompletionItemKind.Enum;
			else if (string.Compare("EnumMember", value, true) == 0)
				return CompletionItemKind.EnumMember;
			else if (string.Compare("Event", value, true) == 0)
				return CompletionItemKind.Event;
			else if (string.Compare("Field", value, true) == 0)
				return CompletionItemKind.Field;
			else if (string.Compare("File", value, true) == 0)
				return CompletionItemKind.File;
			else if (string.Compare("Function", value, true) == 0)
				return CompletionItemKind.Function;
			else if (string.Compare("Interface", value, true) == 0)
				return CompletionItemKind.Interface;
			else if (string.Compare("Keyword", value, true) == 0)
				return CompletionItemKind.Keyword;
			else if (string.Compare("Method", value, true) == 0 || string.Compare("ExtensionMethod", value, true) == 0)
				return CompletionItemKind.Method;
			else if (string.Compare("Module", value, true) == 0)
				return CompletionItemKind.Module;
			else if (string.Compare("Namespace", value, true) == 0)
				return CompletionItemKind.Module;//namespace
			else if (string.Compare("Class", value, true) == 0)
				return CompletionItemKind.Class;
			else if (string.Compare("Null", value, true) == 0)
				return CompletionItemKind.Value;//null
			else if (string.Compare("Number", value, true) == 0)
				return CompletionItemKind.Value;//number
			else if (string.Compare("Reference", value, true) == 0)
				return CompletionItemKind.Reference;//object
			else if (string.Compare("Operator", value, true) == 0)
				return CompletionItemKind.Operator;
			else if (string.Compare("Package", value, true) == 0)
				return CompletionItemKind.Module;//package
			else if (string.Compare("Property", value, true) == 0)
				return CompletionItemKind.Property;
			else if (string.Compare("String", value, true) == 0)
				return CompletionItemKind.Value;//string
			else if (string.Compare("Structure", value, true) == 0)
				return CompletionItemKind.Struct;
			else if (string.Compare("Type", value, true) == 0)
				return CompletionItemKind.TypeParameter;
			else if (string.Compare("Variable", value, true) == 0)
				return CompletionItemKind.Variable;

			return CompletionItemKind.Text;
		}
	}
}