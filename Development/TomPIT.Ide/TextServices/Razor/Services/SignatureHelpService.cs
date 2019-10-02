using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Ide.TextServices.Services;

namespace TomPIT.Ide.TextServices.Razor.Services
{
	internal class SignatureHelpService : RazorEditorService, ISignatureHelpService
	{
		public SignatureHelpService(RazorEditor editor) : base(editor)
		{
		}

		public ISignatureHelp ProvideSignatureHelp(IPosition position, ISignatureHelpContext context)
		{
			var r = new SignatureHelp();

			var model = Editor.Document.GetSemanticModelAsync().Result;
			var completion = CompletionService.GetService(Editor.Document);
			var sm = Editor.Document.GetSemanticModelAsync().Result;
			var caret = Editor.GetMappedCaret(position);
			var token = model.SyntaxTree.GetRoot().FindToken(caret);

			if (token == default)
				return default;

			SyntaxNode node = token.Parent;

			if (node == null || node.Parent == null)
				return r;

			var span = completion.GetDefaultCompletionListSpan(Editor.SourceText, caret);
			SymbolInfo symbols;
			var currentNode = node.Parent;

			while (true)
			{
				symbols = sm.GetSpeculativeSymbolInfo(caret, currentNode, SpeculativeBindingOption.BindAsExpression);

				if (symbols.Symbol != null || symbols.CandidateSymbols.Length > 0)
					break;

				currentNode = currentNode.Parent;

				if (currentNode == null)
					break;
			}

			if (symbols.Symbol != null)
				CreateSignatures(symbols.Symbol, r);

			foreach (var i in symbols.CandidateSymbols)
				CreateSignatures(i, r);

			var list = node as ArgumentListSyntax;

			if (list == null)
				list = node.Parent as ArgumentListSyntax;

			if (list != null)
			{
				foreach (var i in list.Arguments)
				{
					if (i.Span.End < span.End)
						r.ActiveParameter++;
					else
						break;
				}
			}

			if (r.Signatures.Count > 1)
			{
				var exact = list == null
					? r.Signatures.Where(f => f.Parameters.Count == 0)
					: r.Signatures.Where(f => f.Parameters.Count == list.Arguments.Count);

				if (exact.Count() > 0)
					r.ActiveSignature = r.Signatures.IndexOf(exact.ElementAt(0));
				else
				{
					exact = list == null ? r.Signatures : r.Signatures.Where(f => f.Parameters.Count > list.Arguments.Count);

					if (exact.Count() > 0)
						r.ActiveSignature = r.Signatures.IndexOf(exact.ElementAt(0));
				}
			}

			return r;
		}

		private void CreateSignatures(ISymbol symbol, ISignatureHelp signature)
		{
			if (!(symbol is IMethodSymbol mi))
				signature.Signatures.Add(CreateDefaultSignature(symbol));
			else
			{

				var d = new SignatureInformation
				{
					Label = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
					Documentation = symbol.GetDocumentationCommentXml()
				};

				foreach (var i in mi.Parameters)
				{
					d.Parameters.Add(new ParameterInformation
					{
						Label = i.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
						Documentation = symbol.GetDocumentationCommentXml()
					});
				}

				signature.Signatures.Add(d);
			}
		}

		private ISignatureInformation CreateDefaultSignature(ISymbol symbol)
		{
			return new SignatureInformation
			{
				Label = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
				Documentation = symbol.GetDocumentationCommentXml()
			};
		}
	}
}
