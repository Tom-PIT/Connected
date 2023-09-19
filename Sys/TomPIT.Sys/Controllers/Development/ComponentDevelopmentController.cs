using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Sys.Model;
using TomPIT.Sys.Model.Components;

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
			var nameSpace = body.Required<string>("nameSpace");

			DataModel.Components.Insert(component, microService, folder, category, nameSpace, name, type, configuration);
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
		public void UpdateIndexStates()
		{
			return;
			var body = FromBody();
			var items = body.Required<JArray>("items");
			var parameters = new List<IComponentIndexState>();

			foreach (JObject item in items)
			{
				var cmp = DataModel.Components.Select(item.Required<Guid>("component"));

				if (cmp == null)
					continue;

				parameters.Add(new ComponentIndexState
				{
					Component = cmp,
					Element = item.Optional("element", Guid.Empty),
					State = item.Required<IndexState>("state"),
					TimeStamp = item.Required<DateTime>("timestamp")
				});
			}

			DataModel.DevelopmentStates.Update(parameters);
		}

		[HttpPost]
		public void UpdateAnalyzerStates()
		{
			return;
			var body = FromBody();
			var items = body.Required<JArray>("items");
			var parameters = new List<IComponentAnalyzerState>();

			foreach (JObject item in items)
			{
				var cmp = DataModel.Components.Select(item.Required<Guid>("component"));

				if (cmp == null)
					continue;

				parameters.Add(new ComponentAnalyzerState
				{
					Component = cmp,
					Element = item.Optional("element", Guid.Empty),
					State = item.Required<AnalyzerState>("state"),
					TimeStamp = item.Required<DateTime>("timestamp")
				});
			}

			DataModel.DevelopmentStates.Update(parameters);
		}

		public ImmutableArray<IComponentDevelopmentState> DequeueDevelopmentStates()
		{
			var body = FromBody();
			var count = body.Required<int>("count");

			return DataModel.DevelopmentStates.Dequeue(count);
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
			var permanent = body.Required<bool>("permanent");
			var user = body.Required<Guid>("user");

			DataModel.Components.Delete(component, user, permanent);
		}

		[HttpGet]
		public string CreateName(Guid microService, string prefix, string nameSpace)
		{
			return DataModel.Components.CreateComponentName(microService, prefix, nameSpace);
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

				items.Add(new Guid(prop.Name), new Guid(prop.Value.ToString()));
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
