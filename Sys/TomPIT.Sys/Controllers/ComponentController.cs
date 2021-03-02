using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class ComponentController : SysController
	{
		[HttpGet]
		public ImmutableList<IComponent> Query(Guid microService)
		{
			return DataModel.Components.Query(microService, false);
		}

		[HttpGet]
		public ImmutableList<IComponent> QueryAll(Guid microService)
		{
			return DataModel.Components.Query(microService, true);
		}

		[HttpGet]
		public ImmutableList<IComponent> QueryByCategory(Guid microService, string category)
		{
			return DataModel.Components.Query(microService, category);
		}

		[HttpGet]
		public ImmutableList<IComponent> QueryByFolder(Guid microService, Guid folder)
		{
			return DataModel.Components.Query(microService, folder);
		}

		[HttpPost]
		public ImmutableList<IComponent> QueryByResourceGroups()
		{
			var body = FromBody();
			var resourceGroups = body.Required<string>("resourceGroups");
			var categories = body.Optional("categories", string.Empty);

			return DataModel.Components.Query(resourceGroups, categories);
		}

		[HttpGet]
		public ImmutableList<IComponent> QueryByMicroService(Guid microService, string categories)
		{
			return DataModel.Components.QueryCategories(microService, categories);
		}

		[HttpPost]
		public ImmutableList<IComponent> QueryForMicroServices()
		{
			var body = FromBody();
			var list = body.Required<JArray>("microServices");
			var serviceList = new List<Guid>();

			foreach (JValue item in list)
				serviceList.Add(Types.Convert<Guid>(item.Value));

			return DataModel.Components.Query(serviceList.ToArray(), false);
		}

		[HttpGet]
		public IComponent SelectByToken(Guid component)
		{
			return DataModel.Components.Select(component);
		}

		[HttpGet]
		public IComponent Select(Guid microService, string category, string name)
		{
			return DataModel.Components.Select(microService, category, name);
		}

		[HttpGet]
		public IComponent SelectByNameSpace(Guid microService, string nameSpace, string name)
		{
			return DataModel.Components.SelectByNameSpace(microService, nameSpace, name);
		}
	}
}
