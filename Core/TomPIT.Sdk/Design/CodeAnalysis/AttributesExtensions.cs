using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomPIT.Design.CodeAnalysis
{
	public static class AttributesExtensions
	{
		public static AttributeArgumentListSyntax GetArgumentList(this AttributeArgumentSyntax syntax)
		{
			return syntax.Parent as AttributeArgumentListSyntax;
		}

		public static ArgumentListSyntax GetArgumentList(this ArgumentSyntax syntax)
		{
			return syntax.Parent as ArgumentListSyntax;
		}

		public static TypeInfo GetAttribute<T>(this SyntaxList<AttributeListSyntax> attributes, SemanticModel model)
		{
			foreach (var list in attributes)
			{
				if (list is AttributeListSyntax listSyntax)
				{
					foreach (var attribute in listSyntax.Attributes)
					{
						var typeInfo = model.GetTypeInfo(attribute);

						if (typeInfo.IsOfType(typeof(T)))
							return typeInfo;
					}
				}
			}

			return default;
		}

		public static AttributeData GetAttribute<T>(this ImmutableArray<AttributeData> attributes, SemanticModel model)
		{
			foreach (var attribute in attributes)
			{
				if (attribute.AttributeClass is null)
					continue;

				if (attribute.AttributeClass.IsOfType(typeof(T)))
					return attribute;
			}

			return default;
		}

		public static bool ContainsAttribute<T>(this SyntaxList<AttributeListSyntax> attributes, SemanticModel model)
		{
			return attributes.GetAttribute<T>(model).ConvertedType is not null;
		}

		public static bool ContainsAttribute<T>(this ImmutableArray<AttributeData> attributes, SemanticModel model)
		{
			return attributes.GetAttribute<T>(model) is not null;
		}
	}
}
