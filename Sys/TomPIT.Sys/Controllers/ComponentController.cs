using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class ComponentController : SysController
	{
		[HttpGet]
		public List<IComponent> Query(Guid microService)
		{
			return DataModel.Components.Query(microService);
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
		public IComponent SelectByName(string category, string name)
		{
			return DataModel.Components.Select(category, name);
		}
	}
}
