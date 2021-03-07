using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Design.CodeAnalysis;
using TomPIT.Middleware;
using TomPIT.Middleware.Interop;
using TomPIT.Reflection.Manifests.Entities;

namespace TomPIT.Reflection.Manifests.Providers
{
	internal class ApiManifestProvider : ComponentManifestProvider<IApiConfiguration>
	{
		protected override IComponentManifest OnCreateManifest()
		{
			var manifest = new ApiManifest();

			BindManifest(manifest);
			BindApi(manifest);
			BindOperations(manifest);

			return manifest;
		}

		private void BindApi(ApiManifest manifest)
		{
			manifest.Scope = Configuration.Scope;

			var script = ManifestExtensions.GetScript(Tenant, Configuration, null);

			if(script != null)
			{
				var compilation = Tenant.GetService<ICompilerService>().GetCompilation(Configuration);

				if (compilation != null)
				{
					var tree = compilation.SyntaxTrees.FirstOrDefault(f => string.Compare(f.FilePath, $"{manifest.Name}.csx", true) == 0);
					var declaration = tree.FindClass(manifest.Name);

					if (declaration != null)
					{
						var model = compilation.GetSemanticModel(tree);

						manifest.Documentation = ManifestExtensions.ExtractDocumentation(declaration);
					}
				}
			}

			using var ctx = new MicroServiceContext(Configuration.MicroService(), Tenant.Url);
			var info = Tenant.GetService<IDiscoveryService>().MicroServiceInfo(ctx, ctx.MicroService.Token);

			if (info != null)
			{
				manifest.Version = info.Version;
				manifest.TermsOfService = info.TermsOfService;
				
				manifest.License.Name = info.License.Name;
				manifest.License.Url = info.License.Url;

				manifest.Contact.Email = info.Contact.Email;
				manifest.Contact.Name = info.Contact.Name;
				manifest.Contact.Url = info.Contact.Url;
			}
		}

		private void BindOperations(ApiManifest manifest)
		{
			foreach (var operation in Configuration.Operations)
			{
				if (Element != Guid.Empty && Element != operation.Id)
					continue;

				BindOperation(manifest, operation);
			}
		}

		private void BindOperation(ApiManifest manifest, IApiOperation operation)
		{
			var om = new ApiOperationManifest
			{
				Name = operation.Name,
				Scope = operation.Scope,
				Id = operation.Id
			};
			
			manifest.Operations.Add(om);

			var script = ManifestExtensions.GetScript(Tenant, operation, null);

			foreach (var diagnostic in script.Errors)
				om.Diagnostics.Add(ManifestDiagnostic.FromDiagnostic(diagnostic, operation));

			if (om.Diagnostics.HasErrors())
				return;

			var operationType = Tenant.GetService<ICompilerService>().ResolveType(MicroService.Token, operation, operation.Name, false);

			if (operationType == null)
			{
				om.NotImplemented();
				return;
			}

			if (operationType.FindAttribute<SupportsTransactionAttribute>() != null)
				om.SupportsTransaction = true;

			var compilation = Tenant.GetService<ICompilerService>().GetCompilation(operation);

			if (compilation == null)
			{
				om.SyntaxTreeException();
				return;
			}

			var tree = compilation.SyntaxTrees.FirstOrDefault(f => string.Compare(f.FilePath, $"{operation.Name}.csx", true) == 0);
			var declaration = tree.FindClass(operation.Name);
			var model = compilation.GetSemanticModel(tree);
			
			BindType(model, declaration, om, manifest.Types);
			BindExtenders(model, declaration, om);

			var returnSymbol = declaration.LookupBaseType(model, typeof(IOperation<>).FullTypeName());

			if (returnSymbol != null)
			{

				var namedType = returnSymbol as INamedTypeSymbol;
				var argument = namedType.TypeArguments[0] as INamedTypeSymbol;

				if (argument != null)
				{
					om.ReturnType = new ManifestType();

					BindType(model, argument, om.ReturnType, manifest.Types);
				}
			}

			var distributed = declaration.LookupBaseType(model, typeof(IDistributedOperation).FullTypeName());

			if (distributed != null)
				om.Distributed = true;

			if (declaration.AttributeLists.ContainsAttribute<HttpGetAttribute>(model))
				om.Verbs |= HttpVerbs.Get;
			else if (declaration.AttributeLists.ContainsAttribute<HttpPostAttribute>(model))
				om.Verbs |= HttpVerbs.Post;
			else if (declaration.AttributeLists.ContainsAttribute<HttpDeleteAttribute>(model))
				om.Verbs |= HttpVerbs.Delete;
			else if (declaration.AttributeLists.ContainsAttribute<HttpHeadAttribute>(model))
				om.Verbs |= HttpVerbs.Head;
			else if (declaration.AttributeLists.ContainsAttribute<HttpOptionsAttribute>(model))
				om.Verbs |= HttpVerbs.Options;
			else if (declaration.AttributeLists.ContainsAttribute<HttpPatchAttribute>(model))
				om.Verbs |= HttpVerbs.Patch;
			else if (declaration.AttributeLists.ContainsAttribute<HttpPutAttribute>(model))
				om.Verbs |= HttpVerbs.Put;
			else if (declaration.AttributeLists.ContainsAttribute<HttpTraceAttribute>(model))
				om.Verbs |= HttpVerbs.Trace;
		}

		private void BindExtenders(SemanticModel model, ClassDeclarationSyntax syntax, ApiOperationManifest manifest)
		{
			if (syntax == null || syntax.AttributeLists.Count == 0)
				return;

			foreach (var attributeList in syntax.AttributeLists)
			{
				foreach (var attribute in attributeList.Attributes)
				{
					var type = model.GetTypeInfo(attribute);

					if (type.Type == null || !type.Type.IsInheritedFrom(typeof(ExtenderAttribute).FullTypeName()))
						continue;

					var extenderType = new ManifestType();

					//BindType(model, type.Type, extenderType);

					manifest.Extenders.Add(extenderType);
				}
			}
		}
	}
}
