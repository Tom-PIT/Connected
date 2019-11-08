using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using TomPIT.Compilation;
using TomPIT.ComponentModel.IoC;
using TomPIT.Design.CodeAnalysis;
using TomPIT.IoC;
using TomPIT.Reflection.Manifests.Entities;

namespace TomPIT.Reflection.Manifests.Providers
{
	internal class IoCManifestProvider : ComponentManifestProvider<IIoCContainerConfiguration>
	{
		protected override IComponentManifest OnCreateManifest()
		{
			var manifest = new IoCContainerManifest();

			BindManifest(manifest);
			BindContainer(manifest);
			BindOperations(manifest);

			return manifest;
		}

		private void BindContainer(IoCContainerManifest manifest)
		{

		}

		private void BindOperations(IoCContainerManifest manifest)
		{
			foreach (var operation in Configuration.Operations)
			{
				if (Element != Guid.Empty && Element != operation.Id)
					continue;

				BindOperation(manifest, operation);
			}
		}

		private void BindOperation(IoCContainerManifest manifest, IIoCOperation operation)
		{
			var om = new IoCOperationManifest
			{
				Name = operation.Name
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

			var returnSymbol = declaration.LookupBaseType(model, typeof(IIoCOperationMiddleware<>).FullTypeName());

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
		}
	}
}
