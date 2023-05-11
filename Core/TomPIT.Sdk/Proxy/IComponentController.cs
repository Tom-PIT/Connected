using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.ComponentModel;

namespace TomPIT.Proxy
{
	public interface IComponentController
	{
		ImmutableList<IComponent> Query(bool includeDeleted = false);
		ImmutableList<IComponent> Query(Guid microService);
		ImmutableList<IComponent> QueryAll(Guid microService);
		ImmutableList<IComponent> QueryByCategory(Guid microService, string category);
		ImmutableList<IComponent> QueryByFolder(Guid microService, Guid folder);
		ImmutableList<IComponent> QueryByResourceGroups(string resourceGroups, string categories);
		ImmutableList<IComponent> QueryByCategories(params string[] categories);
		ImmutableList<IComponent> QueryByMicroService(Guid microService, string categories);
		ImmutableList<IComponent> QueryForMicroServices(List<Guid> microServices);
		IComponent SelectByToken(Guid component);
		IComponent Select(Guid microService, string category, string name);
		IComponent SelectByNameSpace(Guid microService, string nameSpace, string name);
	}
}
