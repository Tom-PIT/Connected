using System;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Middleware;
using TomPIT.Reflection.Api;

namespace TomPIT.Reflection.Providers
{
	internal class ApiManifestProvider : ComponentManifestProvider<IApiConfiguration>
	{
		protected override IComponentManifest OnCreateManifest()
		{
			var manifest = new ApiManifest(this);

			BindManifest(manifest);
			BindApi(manifest);
			BindOperations(manifest);
			
			return manifest;
		}

		protected override void OnLoadMetaData(IComponentManifest manifest)
		{
			var api = manifest as ApiManifest;
			
			if(Tenant.GetService<IDiscoveryService>().Manifests.SelectScript(Configuration.MicroService(), Configuration.Component, Configuration.Component) is IScriptManifest script)
			{ 
				script.LoadMetaData(Tenant);

				BindDocumentation(api.DeclaredType, script.DeclaredTypes);
			}

			foreach(var operation in api.Operations)
			{
				if( Tenant.GetService<IDiscoveryService>().Manifests.SelectScript(Configuration.MicroService(), Configuration.Component, operation.Id) is IScriptManifest opScript)
				{
					opScript.LoadMetaData(Tenant);

					BindDocumentation(operation.DeclaredType, opScript.DeclaredTypes);
				}
			}
		}

		private void BindApi(ApiManifest manifest)
		{
			manifest.Scope = Configuration.Scope;

			var script = Tenant.GetService<IDiscoveryService>().Manifests.SelectScript(Configuration.MicroService(), Configuration.Component, Configuration.Component);

			if (script != null)
			{
				if (script.DeclaredTypes.FirstOrDefault(f => string.Compare(f.Name, Configuration.ComponentName(), false) == 0) is IManifestType type)
					manifest.Address = script.GetPointer(script.Address);
			}

			using var ctx = new MicroServiceContext(Configuration.MicroService(), Tenant.Url);
			var info = Tenant.GetService<IDiscoveryService>().MicroServices.Info.SelectMiddleware(ctx, ctx.MicroService.Token);

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

			var script = Tenant.GetService<IDiscoveryService>().Manifests.SelectScript(Configuration.MicroService(), Configuration.Component, operation.Id);

			if (script == null)
				return;

			om.Address = script.GetPointer(script.Address);
			om.DeclaredType = CloneType(script.DeclaredTypes.FirstOrDefault(f => string.Compare(f.Name, operation.Name, false) == 0));

			if (om.DeclaredType is IScriptManifestHttpType http)
				om.Verbs = http.Verbs;

			if (om.DeclaredType is IScriptManifestExtenderSupportedType extender && extender.Extenders.Any())
				om.Extenders.AddRange(extender.Extenders);

			if (om.DeclaredType is IScriptManifestReturnType returnType)
				om.ReturnType = returnType.ReturnType;
		}
	}
}
