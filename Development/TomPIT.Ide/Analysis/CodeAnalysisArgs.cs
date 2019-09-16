using System;
using Microsoft.CodeAnalysis;
using TomPIT.ComponentModel;

namespace TomPIT.Ide.Analysis
{
	public class CodeAnalysisArgs : EventArgs
	{
		public CodeAnalysisArgs(IComponent component, SemanticModel model, SyntaxNode node, SymbolInfo symbol, string expressionText)
		{
			Component = component;
			Model = model;
			Symbol = symbol;
			ExpressionText = expressionText;
			Node = node;
		}

		public SyntaxNode Node { get; }
		public IComponent Component { get; }
		public string ExpressionText { get; }
		public SemanticModel Model { get; }
		public SymbolInfo Symbol { get; }
	}
}
