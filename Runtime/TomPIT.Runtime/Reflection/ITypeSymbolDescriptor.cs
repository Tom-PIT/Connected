using Microsoft.CodeAnalysis;

namespace TomPIT.Reflection
{
	public interface ITypeSymbolDescriptor
	{
		string Name { get; }
		INamedTypeSymbol Symbol { get; }

		SyntaxNode Node { get; }
		SemanticModel Model { get; }
		string ContainingType { get; }
	}
}