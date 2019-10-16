using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Design.CodeAnalysis;

namespace TomPIT.Reflection.Manifests.Providers
{
	internal static class ManifestAttributeResolver
	{
		public static Entities.ManifestAttribute Resolve(SemanticModel model, AttributeData attribute)
		{
			if (attribute.AttributeClass == null || !attribute.AttributeClass.IsInheritedFrom(typeof(ValidationAttribute).FullTypeName()))
				return null;

			var result = new Entities.ManifestAttribute();
			//var name = (attribute.Name as IdentifierNameSyntax).Identifier.ValueText;

			//if (name.EndsWith("Attribute"))
			//	name = name[0..^9];

			//result.Name = name;

			//if (attribute.ArgumentList == null)
			//	return result;

			//var arguments = new List<string>();

			//foreach (var argument in attribute.ArgumentList.Arguments)
			//	arguments.Add(argument.Expression.GetText().ToString());

			//result.Description = string.Join(',', arguments.ToArray());

			return result;
		}

		public static Entities.ManifestAttribute Resolve(SemanticModel model, AttributeSyntax attribute)
		{
			var type = model.GetTypeInfo(attribute);

			if (type.Type == null || !type.Type.IsInheritedFrom(typeof(ValidationAttribute).FullTypeName()))
				return null;

			var result = new Entities.ManifestAttribute();
			var name = (attribute.Name as IdentifierNameSyntax).Identifier.ValueText;

			if (name.EndsWith("Attribute"))
				name = name[0..^9];

			result.Name = name;

			if (attribute.ArgumentList == null)
				return result;

			var arguments = new List<string>();

			foreach (var argument in attribute.ArgumentList.Arguments)
				arguments.Add(argument.Expression.GetText().ToString());

			result.Description = string.Join(',', arguments.ToArray());

			return result;
		}
	}
}
