using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Configuration;
using TomPIT.Reflection.Settings;

namespace TomPIT.Reflection.Providers
{
	internal class SettingsManifestProvider : ComponentManifestProvider<ISettingsConfiguration>
	{
		protected override IComponentManifest OnCreateManifest()
		{
			var manifest = new SettingsManifest(this);

			BindManifest(manifest);
			BindMiddleware(manifest);

			return manifest;
		}

		private void BindMiddleware(SettingsManifest manifest)
		{
			var script = Tenant.GetService<IDiscoveryService>().Manifests.SelectScript(Configuration.MicroService(), Configuration.Component, Configuration.Component);

			if (script == null)
				return;

			manifest.DeclaredType = CloneType(Tenant, script.DeclaredTypes.FirstOrDefault(f => string.Compare(f.Name, Configuration.ComponentName(), false) == 0), script);
		}
	}
}
