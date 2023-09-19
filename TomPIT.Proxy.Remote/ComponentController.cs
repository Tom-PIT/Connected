using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.ComponentModel;

namespace TomPIT.Proxy.Remote
{
	internal class ComponentController : IComponentController
	{
		private const string Controller = "Component";
		public ImmutableList<IComponent> Query(bool includeDeleted = false)
		{
			var u = Connection.CreateUrl(Controller, "Query")
				.AddParameter("includeDeleted", includeDeleted);

			return Connection.Get<List<Component>>(u).ToImmutableList<IComponent>();
		}

		public ImmutableList<IComponent> Query(Guid microService)
		{
			var u = Connection.CreateUrl(Controller, "Query")
				.AddParameter("microService", microService)
				.AddParameter("includeDeleted", false);

			return Connection.Get<List<Component>>(u).ToImmutableList<IComponent>();
		}

		public ImmutableList<IComponent> QueryAll(Guid microService)
		{
			var u = Connection.CreateUrl(Controller, "QueryAll")
				.AddParameter("microService", microService)
				.AddParameter("includeDeleted", true);

			return Connection.Get<List<Component>>(u).ToImmutableList<IComponent>();
		}

		public ImmutableList<IComponent> QueryByCategories(params string[] categories)
		{
			var u = Connection.CreateUrl(Controller, "QueryByCategories");

			return Connection.Post<List<Component>>(u, new
			{
				categories = string.Join(", ", categories)
			}).ToImmutableList<IComponent>();
		}

		public ImmutableList<IComponent> QueryByCategory(Guid microService, string category)
		{
			var u = Connection.CreateUrl(Controller, "QueryByCategory")
				.AddParameter("microService", microService)
				.AddParameter("category", category);

			return Connection.Get<List<Component>>(u).ToImmutableList<IComponent>();

		}

		public ImmutableList<IComponent> QueryByFolder(Guid microService, Guid folder)
		{
			var u = Connection.CreateUrl(Controller, "QueryByFolder")
				.AddParameter("microService", microService)
				.AddParameter("folder", folder);

			return Connection.Get<List<Component>>(u).ToImmutableList<IComponent>();
		}

		public ImmutableList<IComponent> QueryByMicroService(Guid microService, string categories)
		{
			var u = Connection.CreateUrl(Controller, "QueryByMicroService")
				.AddParameter("microService", microService)
				.AddParameter("categories", categories);

			return Connection.Get<List<Component>>(u).ToImmutableList<IComponent>();
		}

		public ImmutableList<IComponent> QueryByResourceGroups(string resourceGroups, string categories)
		{
			var u = Connection.CreateUrl(Controller, "QueryByResourceGroups");
			var args = new
			{
				resourceGroups,
				categories
			};

			return Connection.Post<List<Component>>(u, args).ToImmutableList<IComponent>();
		}

		public ImmutableList<IComponent> QueryForMicroServices(List<Guid> microServices)
		{
			var u = Connection.CreateUrl(Controller, "QueryForMicroServices")
				.AddParameter("microServices", microServices)
				.AddParameter("includeDeleted", false);

			return Connection.Get<List<Component>>(u).ToImmutableList<IComponent>();

		}

		public IComponent Select(Guid microService, string category, string name)
		{
			var u = Connection.CreateUrl(Controller, "Select")
				.AddParameter("microService", microService)
				.AddParameter("category", category)
				.AddParameter("name", name);

			return Connection.Get<Component>(u);
		}

		public IComponent SelectByNameSpace(Guid microService, string nameSpace, string name)
		{
			var u = Connection.CreateUrl(Controller, "SelectByNameSpace")
				.AddParameter("microService", microService)
				.AddParameter("namespace", nameSpace)
				.AddParameter("name", name);

			return Connection.Get<Component>(u);

		}

		public IComponent SelectByToken(Guid component)
		{
			var u = Connection.CreateUrl("Component", "SelectByToken")
				.AddParameter("component", component);

			return Connection.Get<Component>(u);
		}
	}
}
