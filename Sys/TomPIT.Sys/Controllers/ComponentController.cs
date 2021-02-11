using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class ComponentController : SysController
	{
		[HttpGet]
		public List<IComponent> Query(Guid microService)
		{
			return DataModel.Components.Query(microService, false);
		}

		[HttpGet]
		public List<IComponent> QueryAll(Guid microService)
		{
			return DataModel.Components.Query(microService, true);
		}

		[HttpGet]
		public List<IComponent> QueryByCategory(Guid microService, string category)
		{
			return DataModel.Components.Query(microService, category);
		}

		[HttpGet]
		public List<IComponent> QueryByFolder(Guid microService, Guid folder)
		{
			return DataModel.Components.Query(microService, folder);
		}

		[HttpGet]
		public List<IComponent> QueryByResourceGroups(string resourceGroups, string categories)
		{
			return DataModel.Components.Query(resourceGroups, categories);
		}

		[HttpGet]
		public List<IComponent> QueryByMicroService(Guid microService, string categories)
		{
			return DataModel.Components.QueryCategories(microService, categories);
		}

		[HttpPost]
		public List<IComponent> QueryForMicroServices()
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
