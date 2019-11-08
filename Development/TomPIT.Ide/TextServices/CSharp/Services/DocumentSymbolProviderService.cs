using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Ide.TextServices.Services;

namespace TomPIT.Ide.TextServices.CSharp.Services
{
	internal class DocumentSymbolProviderService : CSharpEditorService, IDocumentSymbolProviderService
	{
		public DocumentSymbolProviderService(CSharpEditor editor) : base(editor)
		{
		}

		public List<IDocumentSymbol> ProvideDocumentSymbols()
		{
			var model = Editor.Document.GetSemanticModelAsync().Result;
			var nodes = model.SyntaxTree.GetRoot().DescendantNodesAndSelf();
			var result = new List<IDocumentSymbol>();

			foreach (var node in nodes)
			{
				if (node is ExpressionSyntax es)
					CreateSymbol(result, node, model.GetSymbolInfo(es));
			}

			return result;
		}

		private void CreateSymbol(List<IDocumentSymbol> items, SyntaxNode node, SymbolInfo symbol)
		{
			if (symbol.Symbol == null)
				return;

			var location = node.GetLocation().GetLineSpan();
			var range = node.GetLocation().IsInSource
				? new Range
				{
					EndColumn = location.EndLinePosition.Character + 1,
					EndLineNumber = location.EndLinePosition.Line + 1,
					StartColumn = location.StartLinePosition.Character + 1,
					StartLineNumber = location.StartLinePosition.Line + 1
				}
				: null;

			var ds = new DocumentSymbol
			{
				Name = symbol.Symbol.Name,
				Kind = ResolveKind(symbol.Symbol.Kind),
				Range = range
			};

			if (ds.Kind == Languages.SymbolKind.Null)
				return;

			items.Add(ds);
		}

		private Languages.SymbolKind ResolveKind(Microsoft.CodeAnalysis.SymbolKind kind)
		{
			switch (kind)
			{
				case Microsoft.CodeAnalysis.SymbolKind.Alias:
					return Languages.SymbolKind.Null;
				case Microsoft.CodeAnalysis.SymbolKind.ArrayType:
					return Languages.SymbolKind.Array;
				case Microsoft.CodeAnalysis.SymbolKind.Assembly:
					return Languages.SymbolKind.Module;
				case Microsoft.CodeAnalysis.SymbolKind.DynamicType:
					return Languages.SymbolKind.Object;
				case Microsoft.CodeAnalysis.SymbolKind.ErrorType:
					return Languages.SymbolKind.Object;
				case Microsoft.CodeAnalysis.SymbolKind.Event:
					return Languages.SymbolKind.Event;
				case Microsoft.CodeAnalysis.SymbolKind.Field:
					return Languages.SymbolKind.Field;
				case Microsoft.CodeAnalysis.SymbolKind.Label:
					return Languages.SymbolKind.EnumMember;
				case Microsoft.CodeAnalysis.SymbolKind.Local:
					return Languages.SymbolKind.Variable;
				case Microsoft.CodeAnalysis.SymbolKind.Method:
					return Languages.SymbolKind.Method;
				case Microsoft.CodeAnalysis.SymbolKind.NetModule:
					return Languages.SymbolKind.Module;
				case Microsoft.CodeAnalysis.SymbolKind.NamedType:
					return Languages.SymbolKind.Class;
				case Microsoft.CodeAnalysis.SymbolKind.Namespace:
					return Languages.SymbolKind.Namespace;
				case Microsoft.CodeAnalysis.SymbolKind.Parameter:
					return Languages.SymbolKind.TypeParameter;
				case Microsoft.CodeAnalysis.SymbolKind.PointerType:
					return Languages.SymbolKind.TypeParameter;
				case Microsoft.CodeAnalysis.SymbolKind.Property:
					return Languages.SymbolKind.Property;
				case Microsoft.CodeAnalysis.SymbolKind.RangeVariable:
					return Languages.SymbolKind.Variable;
				case Microsoft.CodeAnalysis.SymbolKind.TypeParameter:
					return Languages.SymbolKind.TypeParameter;
				case Microsoft.CodeAnalysis.SymbolKind.Preprocessing:
					return Languages.SymbolKind.Package;
				case Microsoft.CodeAnalysis.SymbolKind.Discard:
					return Languages.SymbolKind.Variable;
				default:
					return Languages.SymbolKind.Null;
			}
		}
	}
}
