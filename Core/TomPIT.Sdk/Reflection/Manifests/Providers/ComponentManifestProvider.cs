using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design.CodeAnalysis;
using TomPIT.Exceptions;
using TomPIT.Reflection.Manifests.Entities;

namespace TomPIT.Reflection.Manifests.Providers
{
	internal abstract class ComponentManifestProvider<C> : TenantObject, IComponentManifestProvider where C : IConfiguration
	{
		private IMicroService _microService = null;
		public IComponentManifest CreateManifest(ITenant tenant, Guid component)
		{
			return CreateManifest(tenant, component, Guid.Empty);
		}

		public IComponentManifest CreateManifest(ITenant tenant, Guid component, Guid element)
		{
			Tenant = tenant;
			Component = Tenant.GetService<IComponentService>().SelectComponent(component);
			Element = element;

			if (Component == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			Configuration = (C)Tenant.GetService<IComponentService>().SelectConfiguration(component);

			return OnCreateManifest();
		}

		protected Guid Element { get; private set; }
		protected IComponent Component { get; private set; }

		protected C Configuration { get; private set; }

		protected IMicroService MicroService
		{
			get
			{
				if (_microService == null)
					_microService = Tenant.GetService<IMicroServiceService>().Select(Component.MicroService);

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

		protected void BindProperties(SemanticModel model, ITypeSymbol symbol, List<ManifestProperty> properties, List<ManifestMember> types)
		{
			var member = ManifestExtensions.BindType(model, symbol, types);

			if (member != null && member.Properties.Count > 0)
			{
				foreach (var property in member.Properties)
				{
					if (properties.FirstOrDefault(f => string.Compare(f.Name, property.Name, false) == 0) != null)
						continue;

					properties.Add(property);
				}
			}
		}

		protected void BindProperties(SemanticModel model, TypeDeclarationSyntax symbol, List<ManifestProperty> properties, List<ManifestMember> types)
		{
			foreach (var member in symbol.Members)
			{
				if (!(member is PropertyDeclarationSyntax pdx))
					continue;

				if (pdx.Modifiers.FirstOrDefault(f => string.Compare(f.ValueText, "public", false) == 0) == new SyntaxToken())//never returns null
					continue;

				var p = new ManifestProperty
				{
					Name = pdx.Identifier.ValueText,
					Type = CodeAnalysisExtentions.ToManifestTypeName(pdx.Type)
				};

				if (pdx.AccessorList == null)
				{
					if (pdx.ExpressionBody is ArrowExpressionClauseSyntax)
						p.CanRead = true;
				}
				else
				{
					foreach (var accessor in pdx.AccessorList.Accessors)
					{
						if (string.Compare(accessor.Keyword.ValueText, "get", false) == 0)
							p.CanRead = true;
						else if (string.Compare(accessor.Keyword.ValueText, "set", false) == 0)
							p.CanWrite = true;
					}
				}
				foreach (var attributeList in pdx.AttributeLists)
				{
					foreach (var attribute in attributeList.Attributes)
					{
						var att = ManifestAttributeResolver.Resolve(model, attribute);

						if (att != null)
							p.Attributes.Add(att);
					}
				}

				p.Documentation = ManifestExtensions.ExtractDocumentation(pdx);

				properties.Add(p);
			}

			foreach (var baseType in symbol.BaseList.Types)
			{
				var type = model.GetTypeInfo(baseType.Type);

				if (type.Type != null)
					BindProperties(model, type.Type, properties, types);
			}
		}

		protected void BindType(SemanticModel model, TypeDeclarationSyntax symbol, ManifestType manifest, List<ManifestMember> types)
		{
			if (symbol == null)
			{
				manifest.ImplementationStatus = ImplementationStatus.Invalid;
				manifest.ImplementationError = "Not implemented";

				return;
			}

			manifest.Documentation = ManifestExtensions.ExtractDocumentation(symbol);

			BindProperties(model, symbol, manifest.Properties, types);
		}

		protected void BindType(SemanticModel model, ITypeSymbol symbol, ManifestType manifest, List<ManifestMember> types)
		{
			if (symbol == null)
			{
				manifest.ImplementationStatus = ImplementationStatus.Invalid;
				manifest.ImplementationError = "Not implemented";

				return;
			}

			manifest.IsArray = symbol.IsArray(model);

			if (symbol.IsArray(model) && symbol is INamedTypeSymbol nt1 && !nt1.TypeArguments.IsEmpty && nt1.TypeArguments[0] is INamedTypeSymbol nt)
				manifest.Name = nt.Name;
			else
				manifest.Name = symbol.Name;

			//manifest.Documentation = ExtractDocumentation(symbol);

			BindProperties(model, symbol, manifest.Properties, types);
		}
	}
}
