using System;
using TomPIT.ComponentModel;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Reflection.Settings;

namespace TomPIT
{
	internal class SettingPropertyRenderer : PropertyRenderer
	{
		private IManifestMiddleware _middleware = null;
		public SettingPropertyRenderer(IMiddlewareContext context, string setting, string propertyName) : base(context, propertyName)
		{
			Setting = setting;
		}


		private string Setting { get; }
		protected override IManifestMiddleware Manifest
		{
			get
			{
				if (_middleware == null)
				{
					var descriptor = ComponentDescriptor.Settings(Context, Setting);

					descriptor.Validate();

					if (!(Context.Tenant.GetService<IDiscoveryService>().Manifests.Select(descriptor.MicroService.Name, ComponentCategories.Settings, descriptor.Component.Name) is SettingsManifest manifest))
						throw new NullReferenceException($"{SR.ErrManifestNull} ({Setting})");

					if (manifest == null)
						throw new NullReferenceException($"{SR.ErrManifestNull} ({Setting})");

					_middleware = manifest;
				}

				return _middleware;
			}
		}
	}
}
