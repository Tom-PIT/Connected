using System;
using System.Linq;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Reflection.Manifests.Entities;

namespace TomPIT
{
	internal class PropertyRenderer
	{
		private StringBuilder _builder = null;
		private ApiOperationManifest _manifest = null;
		private ManifestProperty _property = null;
		public PropertyRenderer(IMiddlewareContext context, string api, string propertyName)
		{
			Context = context;
			Api = api;
			PropertyName = propertyName;

			GenerateContent();
		}


		private IMiddlewareContext Context { get; }
		private string Api { get; }
		private string PropertyName { get; }
		private ApiOperationManifest Manifest
		{
			get
			{
				if (_manifest == null)
				{
					var descriptor = ComponentDescriptor.Api(Context, Api);

					descriptor.Validate();

					if (!(Context.Tenant.GetService<IDiscoveryService>().Manifest(descriptor.MicroService.Name, ComponentCategories.Api, descriptor.Component.Name) is ApiManifest manifest))
						throw new NullReferenceException($"{SR.ErrManifestNull} ({Api})");

					_manifest = manifest.Operations.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

					if (_manifest == null)
						throw new NullReferenceException($"{SR.ErrManifestNull} ({Api})");
				}

				return _manifest;
			}
		}

		private ManifestProperty Property
		{
			get
			{
				if (_property == null)
				{
					_property = Manifest.Properties.FirstOrDefault(f => string.Compare(f.Name, PropertyName, true) == 0);

					if (_property == null)
						throw new NullReferenceException($"{SR.ErrManifestPropertyNull} ({Api}, {PropertyName})");
				}

				return _property;
			}
		}

		private StringBuilder Builder
		{
			get
			{
				if (_builder == null)
					_builder = new StringBuilder();

				return _builder;
			}
		}

		public string Result => Builder.ToString();

		private void GenerateContent()
		{
			Builder.Clear();

			Builder.Append($"<tp-property name =\"{Property.Name}\" data-type=\"{Property.Type}\"");

			ResolveLocalizationString("LocalizedDisplay", "label");
			ResolveLocalizationString("LocalizedCategory", "category");
			ResolveLocalizationString("LocalizedDescription", "description");

			Builder.Append(">");

			var validationAttributes = Property.Attributes.Where(f => f.IsValidation);

			if (validationAttributes.Count() > 0)
			{
				Builder.AppendLine("<tp-validation>");

				foreach (var attribute in validationAttributes)
				{
					Builder.Append($"<tp-validation-attribute type=\"{attribute.Name}\" ");

					if (!string.IsNullOrWhiteSpace(attribute.Description))
						Builder.Append($"description =\"{attribute.Description}\"");

					Builder.Append(">");
					Builder.AppendLine("</tp-validation-attribute>");
				}

				Builder.AppendLine("</tp-validation>");
			}

			Builder.AppendLine("</tp-property>");
		}

		private void ResolveLocalizationString(string attributeName, string propertyName)
		{
			var display = Property.Attributes.FirstOrDefault(f => string.Compare(f.Name, attributeName, true) == 0);

			if (display == null)
				return;

			var description = display.Description.Split(',');

			if (description.Length < 2)
				return;

			var loc = Context.Services.Globalization.GetString(description[0].Trim().Trim('"'), description[1].Trim().Trim('"'));

			if (!string.IsNullOrWhiteSpace(loc))
				Builder.Append($" {propertyName}=\"{loc}\" ");
		}
	}
}
