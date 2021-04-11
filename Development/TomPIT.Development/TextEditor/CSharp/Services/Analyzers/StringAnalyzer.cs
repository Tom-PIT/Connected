using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Design.CodeAnalysis;

namespace TomPIT.Development.TextEditor.CSharp.Services.Analyzers
{
	internal class StringAnalyzer : AttributeAnalyzer
	{
		protected override void OnAnalyze()
		{
			var text = ResolveExpressionText();

			if (string.IsNullOrWhiteSpace(text))
				return;

			text = text.Trim('"');

			if (string.IsNullOrWhiteSpace(text))
			{
				Context.ReportDiagnostic(Diagnostic.Create(Analyzer.GetDescriptor("TP3006"), Context.Node.GetLocation(), "StringTable"));
				return;
			}

			if (ResolveStringTable() is not IStringTableConfiguration stringTable)
				return;

			if (stringTable.Strings.FirstOrDefault(f => string.Compare(f.Key, text, true) == 0) is not IStringResource _)
				Context.ReportDiagnostic(Diagnostic.Create(Analyzer.GetDescriptor("TP3007"), Context.Node.GetLocation(), "String", text, "StringTable"));
		}

		private string ResolveExpressionText()
		{
			if (Context.Node is AttributeArgumentSyntax argument)
			{
				if (argument.Expression.TryResolveValue(Context.SemanticModel, out object value))
					return value as string;
			}
			else if (Context.Node is ArgumentSyntax arg)
			{
				if (arg.Expression.TryResolveValue(Context.SemanticModel, out object value))
					return value as string;
			}

			return null;
		}

		private IStringTableConfiguration ResolveStringTable()
		{
			if (ResolveExpression() is not ExpressionSyntax expression)
				return null;

			if (!expression.TryResolveValue(Context.SemanticModel, out object value))
				return null;

			var text = value.ToString().Trim('"');

			if (string.IsNullOrWhiteSpace(text))
				return null;

			var tokens = text.Split('/');

			if (tokens.Length != 2)
				return null;

			if (Analyzer.Tenant.GetService<IMicroServiceService>().Select(tokens[0]) is not IMicroService ms)
				return null;

			return Analyzer.Tenant.GetService<IComponentService>().SelectConfiguration(ms.Token, ComponentCategories.StringTable, tokens[1]) as IStringTableConfiguration;
		}

		private ExpressionSyntax ResolveExpression()
		{
			if (Context.Node.Parent is ArgumentListSyntax list && list.Arguments.Any())
				return list.Arguments[0].Expression;
			else if (Context.Node.Parent is AttributeArgumentListSyntax alist && alist.Arguments.Any())
				return alist.Arguments[0].Expression;

			return null;

		}
	}
}
