using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TomPIT.Compilation
{
	[DiagnosticAnalyzer("csharp")]
	internal class TestCodeAnalyer : DiagnosticAnalyzer
	{
		private List<DiagnosticDescriptor> _descriptors = null;
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Descriptors.ToImmutableArray();

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
		}

		private void AnalyzeNode(SyntaxNodeAnalysisContext context)
		{
			//var invocationExpr = (InvocationExpressionSyntax)context.Node;
			//var memberAccessExpr =
			//  invocationExpr.Expression as MemberAccessExpressionSyntax;
			//if (memberAccessExpr?.Name.ToString() != "Match") return;
			//var memberSymbol = context.SemanticModel.
			//  GetSymbolInfo(memberAccessExpr).Symbol as IMethodSymbol;
			//if (!memberSymbol?.ToString().StartsWith(
			//  "System.Text.RegularExpressions.Regex.Match") ?? true) return;
			//var argumentList = invocationExpr.ArgumentList as ArgumentListSyntax;
			//if ((argumentList?.Arguments.Count ?? 0) < 2) return;
			//var regexLiteral =
			//  argumentList.Arguments[1].Expression as LiteralExpressionSyntax;
			//if (regexLiteral == null) return;
			//var regexOpt = context.SemanticModel.GetConstantValue(regexLiteral);
			//if (!regexOpt.HasValue) return;
			//var regex = regexOpt.Value as string;
			//if (regex == null) return;
			//try
			//{
			//   System.Text.RegularExpressions.Regex.Match("", regex);
			//}
			//catch (ArgumentException e)
			//{
			var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(Descriptors[0], Microsoft.CodeAnalysis.Location.None, "Ni classa");
			context.ReportDiagnostic(diagnostic);
			//}
		}
		private List<DiagnosticDescriptor> Descriptors
		{
			get
			{
				if (_descriptors == null)
				{
					_descriptors = new List<DiagnosticDescriptor>
					{
						new DiagnosticDescriptor("ComponentClass", "Script should contain class with the same name as Component", null, "Naming", DiagnosticSeverity.Error, true)
					};
				}

				return _descriptors;
			}
		}
	}
}
