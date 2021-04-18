using System.Collections;
using Microsoft.CodeAnalysis;
using TomPIT.Reflection;

namespace TomPIT.Design.CodeAnalysis
{
	public static class ArrayExtensions
	{
		public static bool IsArray(this ITypeSymbol type, SemanticModel model)
		{
			return type.LookupBaseType(model, typeof(IEnumerable).FullTypeName()) != default;
		}
	}
}
