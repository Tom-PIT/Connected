using Microsoft.CodeAnalysis;

namespace TomPIT.Reflection.CodeAnalysis
{
	internal class TypeSymbolDescriptor : ITypeSymbolDescriptor
	{
		public string Name { get; set; }
		public INamedTypeSymbol Symbol { get; set; }
		
		public SyntaxNode Node { get; set; }
		public SemanticModel Model { get; set; }
	}
}