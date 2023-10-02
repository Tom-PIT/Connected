using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class ComponentController : IComponentController
	{
		public ImmutableList<IComponent> Query()
		{
			return DataModel.Components.Query();
		}

		public ImmutableList<IComponent> Query(Guid microService)
		{
			return DataModel.Components.Query(microService);
		}

		public ImmutableList<IComponent> QueryAll(Guid microService)
		{
			return DataModel.Components.Query(microService);
		}

		public ImmutableList<IComponent> QueryByCategories(params string[] categories)
		{
			return DataModel.Components.QueryByCategories(string.Join(',', categories));
		}

		public ImmutableList<IComponent> QueryByCategory(Guid microService, string category)
		{
			return DataModel.Components.Query(microService, category);
		}

		public ImmutableList<IComponent> QueryByFolder(Guid microService, Guid folder)
		{
			return DataModel.Components.Query(microService, folder);
		}

		public ImmutableList<IComponent> QueryByMicroService(Guid microService, string categories)
		{
			return DataModel.Components.QueryCategories(microService, categories);
		}

		public ImmutableList<IComponent> QueryByResourceGroups(string resourceGroups, string categories)
		{
			return DataModel.Components.Query(resourceGroups, categories);
		}

		public ImmutableList<IComponent> QueryForMicroServices(List<Guid> microServices)
		{
			return DataModel.Components.Query(microServices.ToArray());
		}

		public IComponent Select(Guid microService, string category, string name)
		{
			return DataModel.Components.Select(microService, category, name);
		}

		public IComponent SelectByNameSpace(Guid microService, string nameSpace, string name)
		{
			return DataModel.Components.SelectByNameSpace(microService, nameSpace, name);
		}

		public IComponent SelectByToken(Guid component)
		{
			return DataModel.Components.Select(component);
		}
	}
}
