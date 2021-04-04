using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;
using TomPIT.Design.CodeAnalysis;

namespace TomPIT.Reflection.CodeAnalysis
{
	internal class AttributeParser : ParserBase
	{
		public AttributeParser(IScriptManifestCompiler compiler) : base(compiler)
		{
		}

		public ManifestAttribute Parse(AttributeData attribute)
		{
			var type = Compiler.Model.GetTypeInfo(attribute.ApplicationSyntaxReference.GetSyntax());

			if (type.Type == null)
				return null;

			var result = new ManifestAttribute();
			var name = attribute.AttributeClass.Name;

			if (name.EndsWith("Attribute"))
				name = name[0..^9];

			result.Name = name;
			result.IsValidation = type.Type.IsInheritedFrom(typeof(ValidationAttribute).FullTypeName());

			var arguments = new List<string>();

			foreach (var argument in attribute.ConstructorArguments)
			{
				if (argument.Value == null)
					continue;

				arguments.Add(argument.Value.ToString());
			}

			foreach (var argument in attribute.NamedArguments)
			{
				if (argument.Value.IsNull)
					continue;

				arguments.Add($"{argument.Key}={argument.Value.Value}");
			}

			result.Description = string.Join(',', arguments.ToArray());

			return result;
		}
	}
}
