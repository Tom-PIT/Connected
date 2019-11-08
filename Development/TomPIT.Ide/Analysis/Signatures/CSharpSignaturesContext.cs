using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Ide.Analysis.Analyzers;
using TomPIT.Middleware;

namespace TomPIT.Ide.Analysis.Signatures
{
	internal class CSharpSignaturesContext : CSharpCodeAnalyzer<CodeStateArgs>
	{
		private ISignatureInfo _signature = null;

		public CSharpSignaturesContext(IMiddlewareContext context, CodeStateArgs e) : base(context, e)
		{
		}

		public ISignatureInfo Signature
		{
			get
			{
				if (_signature == null)
					_signature = CreateSignature();

				return _signature;
			}
		}

		private ISignatureInfo CreateSignature()
		{
			var r = new SignatureInfo();

			var span = Completion.GetDefaultCompletionListSpan(SourceCode, Args.Position);
			var sm = Task.Run(async () => { return await Document.GetSemanticModelAsync(); }).Result;
			var node = sm.SyntaxTree.GetRoot().FindNode(span);

			if (node == null || node.Parent == null)
				return r;

			SymbolInfo symbols;
			var currentNode = node.Parent;

			while (true)
			{
				symbols = sm.GetSpeculativeSymbolInfo(Args.Position, currentNode, SpeculativeBindingOption.BindAsExpression);

				if (symbols.Symbol != null || symbols.CandidateSymbols.Length > 0)
					break;

				currentNode = currentNode.Parent;

				if (currentNode == null)
					break;
			}

			if (symbols.Symbol != null)
				r.Signatures.AddRange(CreateSignatures(symbols.Symbol));

			foreach (var i in symbols.CandidateSymbols)
				r.Signatures.AddRange(CreateSignatures(i));

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
					exact = r.Signatures.Where(f => f.Parameters.Count > list.Arguments.Count);

					if (exact.Count() > 0)
						r.ActiveSignature = r.Signatures.IndexOf(exact.ElementAt(0));
				}
			}

			return r;
		}

		private List<ISignature> CreateSignatures(ISymbol symbol)
		{
			var r = new List<ISignature>();

			if (!(symbol is IMethodSymbol mi))
				r.Add(CreateDefaultSignature(symbol));
			else
			{

				var d = new Signature
				{
					Label = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
					Documentation = symbol.GetDocumentationCommentXml()
				};

				foreach (var i in mi.Parameters)
				{
					d.Parameters.Add(new SignatureParameter
					{
						Label = i.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
						Documentation = symbol.GetDocumentationCommentXml()
					});
				}

				r.Add(d);
			}

			return r;
		}

		private ISignature CreateDefaultSignature(ISymbol symbol)
		{
			return new Signature
			{
				Label = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
				Documentation = symbol.GetDocumentationCommentXml()
			};
		}
	}
}
