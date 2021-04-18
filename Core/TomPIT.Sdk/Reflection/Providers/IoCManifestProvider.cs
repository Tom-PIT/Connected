using System;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.Reflection.IoC;

namespace TomPIT.Reflection.Providers
{
	internal class IoCManifestProvider : ComponentManifestProvider<IIoCContainerConfiguration>
	{
		protected override IComponentManifest OnCreateManifest()
		{
			var manifest = new IoCContainerManifest(this);

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

			var script = Tenant.GetService<IDiscoveryService>().Manifests.SelectScript(Configuration.MicroService(), Configuration.Component, operation.Id);

			if (script == null)
				return;

			om.DeclaredType = CloneType(Tenant, script.DeclaredTypes.FirstOrDefault(f => string.Compare(f.Name, om.Name, false) == 0), script);
		}
	}
}
