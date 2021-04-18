using Microsoft.CodeAnalysis;

namespace TomPIT.Design.CodeAnalysis
{
	public class AttributeAnalyzerContext
	{
		public AttributeAnalyzerContext(SyntaxNode node, SemanticModel model)
		{
			Node = node;
			Model = model;
		}

		public SyntaxNode Node { get; }
		public SemanticModel Model { get; }
	}
}
