using System;
using TomPIT.ComponentModel;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Reflection.Manifests.Entities;

namespace TomPIT
{
	internal class SettingPropertyRenderer : PropertyRenderer
	{
		private SettingsManifest _manifest = null;
		public SettingPropertyRenderer(IMiddlewareContext context, string setting, string propertyName) : base(context, propertyName)
		{
			Setting = setting;
		}


		private string Setting { get; }
		protected override ManifestType Manifest
		{
			get
			{
				if (_manifest == null)
				{
					var descriptor = ComponentDescriptor.Settings(Context, Setting);

					descriptor.Validate();

					if (!(Context.Tenant.GetService<IDiscoveryService>().Manifest(descriptor.MicroService.Name, ComponentCategories.Settings, descriptor.Component.Name) is SettingsManifest manifest))
						throw new NullReferenceException($"{SR.ErrManifestNull} ({Setting})");

					_manifest = manifest;

					if (_manifest == null)
						throw new NullReferenceException($"{SR.ErrManifestNull} ({Setting})");
				}

				return _manifest.Type;
			}
		}
	}
}
