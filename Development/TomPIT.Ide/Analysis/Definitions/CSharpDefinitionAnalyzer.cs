using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Ide.Analysis.Analyzers;
using TomPIT.Middleware;

namespace TomPIT.Ide.Analysis.Definitions
{
	internal class CSharpDefinitionAnalyzer : CSharpCodeAnalyzer<CodeStateArgs>
	{
		private ILocation _hover = null;

		public CSharpDefinitionAnalyzer(IMiddlewareContext context, CodeStateArgs e) : base(context, e)
		{
		}

		public ILocation Location
		{
			get
			{
				if (_hover == null)
					_hover = CreateDefinition();

				return _hover;
			}
		}

		private ILocation CreateDefinition()
		{
			var sm = Task.Run(async () => { return await Document.GetSemanticModelAsync(); }).Result;
			var span = Completion.GetDefaultCompletionListSpan(SourceCode, Args.Position);
			var node = sm.SyntaxTree.GetRoot().FindNode(span);
			SyntaxToken syntax = new SyntaxToken();

			if (node is IdentifierNameSyntax ins)
			{
				var text = ins.Identifier.ValueText;
				var nodes = sm.SyntaxTree.GetRoot().DescendantNodes();

				foreach (var i in nodes)
				{
					if (i is FieldDeclarationSyntax fd)
					{
						var declaration = fd.Declaration;

						foreach (var j in declaration.Variables)
						{
							if (string.Compare(j.Identifier.Text, text, false) == 0)
							{
								syntax = j.Identifier;

								break;
							}
						}

						if (!syntax.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.None))
							break;
					}
				}

			}

			if (syntax.IsMissing)
				return null;

			var ls = syntax.GetLocation().GetLineSpan();

			return new Location
			{
				Range = new Range
				{
					EndColumn = syntax.Span.End,
					StartColumn = syntax.Span.Start,
					StartLineNumber = ls.StartLinePosition.Line + 1,
					EndLineNumber = ls.EndLinePosition.Line + 1
				},
				Uri = Shell.HttpContext.Request.Path
			};
		}
	}
}
