using Microsoft.CodeAnalysis;

namespace TomPIT.Reflection
{
	public interface ITypeSymbolDescriptor
	{
		string Name { get; }
		string MetaDataName { get; set; }
		INamedTypeSymbol Symbol { get; }

		SyntaxNode Node { get; }
		SemanticModel Model { get; }
		string ContainingType { get; }
		string BaseType { get; }
		string BaseTypeMetaDataName { get; }
	}
}