using Microsoft.AspNetCore.Mvc;
using System;
using TomPIT.Sys.Data;

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
			var configuration = body.Optional("runtimeConfiguration", Guid.Empty);
			var component = body.Required<Guid>("component");

			DataModel.Components.Insert(component, microService, folder, category, name, type, configuration);
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
		public void UpdateRuntimeConfiguration()
		{
			var body = FromBody();

			var component = body.Required<Guid>("component");
			var config = body.Required<Guid>("runtimeConfiguration");

			DataModel.Components.Update(component, config);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var component = body.Required<Guid>("component");

			DataModel.Components.Delete(component);
		}

		[HttpGet]
		public string CreateName(Guid microService, string prefix, string category)
		{
			return DataModel.Components.CreateComponentName(microService, prefix, category);
		}
	}
}
