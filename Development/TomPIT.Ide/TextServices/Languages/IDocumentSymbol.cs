using System.Collections.Generic;

namespace TomPIT.Ide.TextServices.Languages
{
	public enum SymbolTag
	{
		Deprecated = 1
	}
	public enum SymbolKind
	{
		File = 0,
		Module = 1,
		Namespace = 2,
		Package = 3,
		Class = 4,
		Method = 5,
		Property = 6,
		Field = 7,
		Constructor = 8,
		Enum = 9,
		Interface = 10,
		Function = 11,
		Variable = 12,
		Constant = 13,
		String = 14,
		Number = 15,
		Boolean = 16,
		Array = 17,
		Object = 18,
		Key = 19,
		Null = 20,
		EnumMember = 21,
		Struct = 22,
		Event = 23,
		Operator = 24,
		TypeParameter = 25
	}
	public interface IDocumentSymbol
	{
		List<IDocumentSymbol> Children { get; }
		string ContainerName { get; }
		string Detail { get; }
		SymbolKind Kind { get; }
		string Name { get; }
		IRange Range { get; }
		IRange SelectionRange { get; }
		List<SymbolTag> Tags { get; }
	}
}
