using System.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel.Configuration;
using TomPIT.Design.CodeAnalysis;
using TomPIT.Reflection.Manifests.Entities;

namespace TomPIT.Reflection.Manifests.Providers
{
	internal class SettingsManifestProvider : ComponentManifestProvider<ISettingsConfiguration>
	{
		protected override IComponentManifest OnCreateManifest()
		{
			var manifest = new SettingsManifest();

			manifest.Type = new ManifestType();

			BindManifest(manifest);
			BindMiddleware(manifest);

			return manifest;
		}

		private void BindMiddleware(SettingsManifest manifest)
		{
			var script = ManifestExtensions.GetScript(Tenant, Configuration, null);
			var type = Tenant.GetService<ICompilerService>().ResolveType(MicroService.Token, Configuration, manifest.Name, false);

			if (type == null)
			{
				manifest.Type.NotImplemented();
				return;
			}

			foreach (var diagnostic in script.Errors)
				manifest.Type.Diagnostics.Add(ManifestDiagnostic.FromDiagnostic(diagnostic, Configuration));

			if (manifest.Type.Diagnostics.HasErrors())
				return;

			var compilation = Tenant.GetService<ICompilerService>().GetCompilation(Configuration);

			if (compilation == null)
			{
				manifest.Type.SyntaxTreeException();
				return;
			}

			var tree = compilation.SyntaxTrees.FirstOrDefault(f => string.Compare(f.FilePath, $"{manifest.Name}.csx", true) == 0);
			var declaration = tree.FindClass(manifest.Name);
			var model = compilation.GetSemanticModel(tree);

			BindType(model, declaration, manifest.Type, manifest.Types);
		}
	}
}
