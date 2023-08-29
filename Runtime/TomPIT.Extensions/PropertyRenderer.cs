using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using TomPIT.Middleware;

namespace TomPIT
{
	internal abstract class PropertyRenderer
	{
		private StringBuilder _builder = null;
		private PropertyDeclarationSyntax _property = null;
		public PropertyRenderer(IMiddlewareContext context, string propertyName)
		{
			Context = context;

			PropertyName = propertyName;
		}

		protected string PropertyName { get; }

		protected IMiddlewareContext Context { get; }

		protected abstract ImmutableArray<PropertyDeclarationSyntax> Properties { get; }

		private PropertyDeclarationSyntax Property
		{
			get
			{
				if (_property == null)
				{
					_property = Properties.FirstOrDefault(f => string.Equals(f.Identifier.Text, PropertyName, StringComparison.Ordinal));

					if (_property is null)
						throw new NullReferenceException($"{SR.ErrManifestPropertyNull} ({PropertyName})");
				}

				return _property;
			}
		}

		private StringBuilder Builder => _builder ??= new();

		public string Result
		{
			get
			{
				GenerateContent();

				return Builder.ToString();
			}
		}

		private void GenerateContent()
		{
			Builder.Clear();

			Builder.Append($"<tp-property name =\"{Property.Identifier.Text}\" data-type=\"{Property.Type.ToString()}\"");

			ResolveLocalizationString("LocalizedDisplay", "label");
			ResolveLocalizationString("LocalizedCategory", "category");
			ResolveLocalizationString("LocalizedDescription", "description");

			Builder.Append(">");

			if (Property.AttributeLists.Any())
			{
				Builder.AppendLine("<tp-validation>");

				foreach (var list in Property.AttributeLists)
				{
					foreach (var attribute in list.Attributes)
						RenderAttribute(attribute);
				}

				Builder.AppendLine("</tp-validation>");
			}

			Builder.AppendLine("</tp-property>");
		}

		private void RenderAttribute(AttributeSyntax attribute)
		{
			if (string.Equals(attribute.Name.ToString(), "Required", StringComparison.Ordinal))
				RenderRequired(attribute);
			else if (string.Equals(attribute.Name.ToString(), "MaxLength", StringComparison.Ordinal))
				RenderMaxLength(attribute);
			else if (string.Equals(attribute.Name.ToString(), "MinValue", StringComparison.Ordinal))
				RenderMinValue(attribute);
			else if (string.Equals(attribute.Name.ToString(), "MaxValue", StringComparison.Ordinal))
				RenderMaxValue(attribute);
		}

		private void RenderRequired(AttributeSyntax attribute)
		{
			RenderValidation("Required", null);
		}

		private void RenderMaxLength(AttributeSyntax attribute)
		{
			RenderValidation("MaxLength", attribute.ArgumentList.Arguments[0].ToString());
		}

		private void RenderMinValue(AttributeSyntax attribute)
		{
			RenderValidation("MinValue", attribute.ArgumentList.Arguments[0].ToString());
		}

		private void RenderMaxValue(AttributeSyntax attribute)
		{
			RenderValidation("MaxValue", attribute.ArgumentList.Arguments[0].ToString());
		}

		private void RenderValidation(string name, string description)
		{
			Builder.Append($"<tp-validation-attribute type=\"{name}\" ");

			if (!string.IsNullOrWhiteSpace(description))
				Builder.Append($"description =\"{description}\"");

			Builder.Append(">");
			Builder.AppendLine("</tp-validation-attribute>");
		}

		private void ResolveLocalizationString(string attributeName, string propertyName)
		{
			var attribute = FindAttribute(attributeName);

			if (attribute is null)
				return;

			var stringTable = attribute.ArgumentList.Arguments[0].ToString();
			var key = attribute.ArgumentList.Arguments[0].ToString();
			var loc = Context.Services.Globalization.GetString(stringTable, key);

			if (!string.IsNullOrWhiteSpace(loc))
				Builder.Append($" {propertyName}=\"{loc}\" ");
		}

		private AttributeSyntax FindAttribute(string name)
		{
			foreach (var list in Property.AttributeLists)
			{
				foreach (var attribute in list.Attributes)
				{
					if (string.Equals(attribute.Name.ToString(), name, StringComparison.Ordinal))
						return attribute;
				}
			}

			return null;
		}
	}
}
