using System;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Reflection.Manifests.Entities;

namespace TomPIT
{
	internal class ApiPropertyRenderer : PropertyRenderer
	{
		private ApiOperationManifest _manifest = null;
		public ApiPropertyRenderer(IMiddlewareContext context, string api, string propertyName) : base(context, propertyName)
		{
			Api = api;
		}


		private string Api { get; }
		protected override ManifestType Manifest
		{
			get
			{
				if (_manifest == null)
				{
					var descriptor = ComponentDescriptor.Api(Context, Api);

					descriptor.Validate();

					if (!(Context.Tenant.GetService<IDiscoveryService>().Manifests.Select(descriptor.MicroService.Name, ComponentCategories.Api, descriptor.Component.Name) is ApiManifest manifest))
						throw new NullReferenceException($"{SR.ErrManifestNull} ({Api})");

					_manifest = manifest.Operations.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

					if (_manifest == null)
						throw new NullReferenceException($"{SR.ErrManifestNull} ({Api})");
				}

				return _manifest;
			}
		}
	}
}
