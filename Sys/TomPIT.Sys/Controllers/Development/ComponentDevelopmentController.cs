using Microsoft.AspNetCore.Mvc;
using System;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers.Development
{
	public class ComponentDevelopmentController : SysController
	{
		[HttpPost]
		public void Insert()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var folder = body.Optional("folder", Guid.Empty);
			var name = body.Required<string>("name");
			var type = body.Required<string>("type");
			var category = body.Required<string>("category");
			var component = body.Required<Guid>("component");
			var nameSpace = body.Required<string>("nameSpace");

			DataModel.Components.Insert(component, microService, folder, category, nameSpace, name, type);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();

			var component = body.Required<Guid>("component");
			var name = body.Required<string>("name");
			var folder = body.Optional("folder", Guid.Empty);

			DataModel.Components.Update(component, name, folder);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var component = body.Required<Guid>("component");
			var user = body.Required<Guid>("user");

			DataModel.Components.Delete(component, user);
		}

		[HttpGet]
		public string CreateName(Guid microService, string prefix, string nameSpace)
		{
			return DataModel.Components.CreateComponentName(microService, prefix, nameSpace);
		}
	}
}
