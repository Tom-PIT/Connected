using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Analysis.Manifest.Entities;
using TomPIT.ComponentModel.Compilation;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Analysis.Manifest.Providers
{
	internal abstract class ComponentManifestProvider<C> : IComponentManifestProvider where C : IConfiguration
	{
		private IMicroService _microService = null;
		public IComponentManifest CreateManifest(ISysConnection connection, Guid component)
		{
			Connection = connection;

			Component = Connection.GetService<IComponentService>().SelectComponent(component);

			if (Component == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			Configuration = (C)Connection.GetService<IComponentService>().SelectConfiguration(component);

			return OnCreateManifest();
		}

		protected ISysConnection Connection { get; private set; }

		protected IComponent Component { get; private set; }

		protected C Configuration { get; private set; }

		protected IMicroService MicroService
		{
			get
			{
				if (_microService == null)
					_microService = Connection.GetService<IMicroServiceService>().Select(Component.MicroService);

				return _microService;
			}
		}

		protected abstract IComponentManifest OnCreateManifest();

		protected void BindManifest(ComponentManifest manifest)
		{
			manifest.MicroService = MicroService.Name;
			manifest.Name = Component.Name;
			manifest.Category = Component.Category;
		}

		protected SyntaxTree CreateSyntaxTree(IScriptContext context)
		{
			if (string.IsNullOrWhiteSpace(context.SourceCode))
				return null;

			return CSharpSyntaxTree.ParseText(context.SourceCode);
		}

		protected string ExtractDocumentation(CSharpSyntaxNode node)
		{
			var trivias = node.GetLeadingTrivia();

			if (trivias == null || trivias.Count == 0)
				return null;

			var enumerator = trivias.GetEnumerator();

			while (enumerator.MoveNext())
			{
				var trivia = enumerator.Current;

				if (trivia.Kind().Equals(SyntaxKind.SingleLineDocumentationCommentTrivia))
				{
					var xml = trivia.GetStructure();

					if (xml == null)
						continue;

					var fullString = xml.ToFullString();
					var sb = new StringBuilder();
					string currentLine = null;

					using (var r = new StringReader(fullString))
					{
						while ((currentLine = r.ReadLine()) != null)
						{
							currentLine = currentLine.Trim();

							if (currentLine.StartsWith("///"))
								currentLine = currentLine.Substring(3).Trim();

							sb.AppendLine(currentLine);
						}
					}

					return sb.ToString();
				}
			}

			return null;
		}

		protected void BindProperties(SyntaxNode scope, Type type, List<ManifestProperty> properties)
		{
			var props = type.GetProperties();

			foreach (var property in props)
			{
				if (!property.CanWrite || !property.SetMethod.IsPublic || !property.CanRead || !property.GetMethod.IsPublic)
					continue;

				var p = new ManifestProperty
				{
					Name = property.Name,
					Type = property.PropertyType.ToFriendlyName(),
				};

				var skipValidation = property.FindAttribute<SkipValidationAttribute>();

				if (skipValidation == null)
				{
					var attributes = property.GetCustomAttributes(true);

					if (attributes != null)
					{
						foreach (var attribute in attributes)
						{
							if (attribute is ValidationAttribute va)
								p.Validation.Add(ManifestValidationAttributeResolver.Resolve(va));
						}
					}
				}

				var propertyNode = scope.FindProperty(property.Name);

				if (propertyNode != null)
					p.Documentation = ExtractDocumentation(propertyNode);

				properties.Add(p);
			}
		}

		protected void BindType(SyntaxNode scope, Type type, ManifestType manifest)
		{
			var classDeclaration = scope.FindClass(type.Name);

			if (classDeclaration == null)
			{
				manifest.ImplementationStatus = ImplementationStatus.Invalid;
				manifest.ImplementationError = "Not implemented";

				return;
			}

			manifest.Documentation = ExtractDocumentation(classDeclaration);

			BindProperties(scope, type, manifest.Properties);
		}
	}
}