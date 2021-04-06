using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Exceptions;

namespace TomPIT.Reflection.Providers
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

		protected void BindManifest(ComponentManifestMiddleware manifest)
		{
			manifest.MicroService = MicroService.Name;
			manifest.Name = Component.Name;
			manifest.Category = Component.Category;
		}

		public void LoadMetaData(IComponentManifest manifest)
		{
			OnLoadMetaData(manifest);
		}

		protected virtual void OnLoadMetaData(IComponentManifest manifest)
		{

		}

		protected void BindDocumentation(List<IManifestType> manifestTypes, List<IScriptManifestType> scriptTypes)
		{
			foreach (var type in manifestTypes)
				BindDocumentation(type, scriptTypes);
		}

		protected void BindDocumentation(IManifestType manifestType, List<IScriptManifestType> scriptTypes)
		{
			if (manifestType is null)
				return;

			if (scriptTypes.FirstOrDefault(f => string.Compare(f.Name, manifestType.Name, false) == 0) is not IManifestType scriptType)
				return;

			manifestType.Documentation = scriptType.Documentation;

			foreach (var member in manifestType.Members)
			{
				if (scriptType.Members.FirstOrDefault(f => string.Compare(f.Name, member.Name, true) == 0) is IManifestMember scriptMember)
					member.Documentation = scriptMember.Documentation;
			}
		}

		protected IManifestType CloneType(IScriptManifestType type)
		{
			if (type is null)
				return null;
			
			return ManifestType.FromScript(type);
		}
	}
}