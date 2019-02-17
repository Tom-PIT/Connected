using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

		[HttpPost]
		public void SaveRuntimeState()
		{
			var body = FromBody();
			var ms = body.Required<Guid>("microService");
			var state = body.Required<JArray>("runtimeConfigurations");
			var items = new Dictionary<Guid, Guid>();

			foreach (JObject i in state)
			{
				var prop = i.First as JProperty;

				items.Add(prop.Name.AsGuid(), prop.Value.ToString().AsGuid());
			}

			DataModel.Components.SaveRuntimeState(ms, items);
		}

		[HttpPost]
		public JArray SelectRuntimeState()
		{
			var body = FromBody();
			var ms = body.Required<Guid>("microService");

			return DataModel.Components.SelectRuntimeState(ms, out Guid id);
		}

		[HttpPost]
		public void DropRuntimeState()
		{
			var body = FromBody();
			var ms = body.Required<Guid>("microService");

			DataModel.Components.DropRuntimeState(ms);
		}
	}
}
