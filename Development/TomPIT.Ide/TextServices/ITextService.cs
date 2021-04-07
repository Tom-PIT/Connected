using System;
using TomPIT.Middleware;

namespace TomPIT.Ide.TextServices
{
	[Flags]
	public enum LanguageFeature
	{
		None = 0,
		CheckSyntax = 1,
		CodeAction = 2,
		CodeLens = 4,
		Color = 8,
		CompletionItem = 16,
		Declaration = 32,
		Definition = 64,
		DocumentFormatting = 128,
		DocumentHighlight = 256,
		DocumentRangeFormatting = 512,
		DocumentSymbol = 1024,
		FoldingRange = 2048,
		Hover = 4096,
		Implementation = 8192,
		Link = 16384,
		OnTypeFormatting = 32768,
		Reference = 65536,
		Rename = 131072,
		SelectionRange = 262144,
		SignatureHelp = 524288,
		TypeDefinition = 1048576,
		DeltaDecorations = 2097152,
		All = 2097151
	}

	public interface ITextService
	{
		ITextEditor GetEditor(IMicroServiceContext context, string language);

		void RegisterEditor(string language, Type editorType);
	}
}
