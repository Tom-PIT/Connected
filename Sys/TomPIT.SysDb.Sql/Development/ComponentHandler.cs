using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;
using TomPIT.Development;
using TomPIT.Security;
using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class ComponentHandler : IComponentHandler
	{
		public void Delete(IComponent component)
		{
			using var w = new Writer("tompit.component_del");

			w.CreateParameter("@id", component.GetId());

			w.Execute();
		}

		public void Insert(IMicroService service, DateTime modified, IFolder folder, string category, string nameSpace, string name, Guid token, string type, Guid runtimeConfiguration)
		{
			using var w = new Writer("tompit.component_ins");

			w.CreateParameter("@folder", folder == null ? 0 : folder.GetId(), true);
			w.CreateParameter("@name", name);
			w.CreateParameter("@token", token);
			w.CreateParameter("@type", type);
			w.CreateParameter("@category", category);
			w.CreateParameter("@runtime_configuration", runtimeConfiguration, true);
			w.CreateParameter("@service", service.GetId());
			w.CreateParameter("@modified", modified);
			w.CreateParameter("@namespace", nameSpace);

			w.Execute();
		}

		public List<IComponent> Query()
		{
			using var r = new Reader<Component>("tompit.component_que");

			return r.Execute().ToList<IComponent>();
		}

		public IComponent Select(Guid component)
		{
			using var r = new Reader<Component>("tompit.component_sel");

			r.CreateParameter("@component", component);

			return r.ExecuteSingleRow();
		}

		public List<IComponent> Query(string category, string name)
		{
			using var r = new Reader<Component>("tompit.component_que");

			r.CreateParameter("@name", name);
			r.CreateParameter("@category", category);

			return r.Execute().ToList<IComponent>();
		}

		public IComponent Select(IMicroService microService, string category, string name)
		{
			using var r = new Reader<Component>("tompit.component_sel");

			r.CreateParameter("@service", microService.GetId());
			r.CreateParameter("@name", name);
			r.CreateParameter("@category", category);

			return r.ExecuteSingleRow();
		}

		public void Update(IComponent component, DateTime modified, string name, IFolder folder, Guid runtimeConfiguration)
		{
			using var w = new Writer("tompit.component_upd");

			w.CreateParameter("@id", component.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@modified", modified);
			w.CreateParameter("@folder", folder == null ? 0 : folder.GetId(), true);
			w.CreateParameter("@runtime_configuration", runtimeConfiguration, true);

			w.Execute();
		}

		public void Update(IComponent component, IUser user, LockStatus status, LockVerb verb, DateTime date)
		{
			using var w = new Writer("tompit.component_lock_upd");

			w.CreateParameter("@id", component.GetId());
			w.CreateParameter("@lock_status", status);
			w.CreateParameter("@lock_user", user == null ? 0 : user.GetId(), true);
			w.CreateParameter("@lock_date", date, true);
			w.CreateParameter("@lock_verb", verb);

			w.Execute();
		}

		public void UpdateStates(List<IComponentIndexState> states)
		{
			var itemsParameter = new JArray();

			foreach (var state in states)
			{
				var parameter = new JObject
				{
					{"id", state.Component.GetId().ToString() },
					{"index_state", (int)state.State },
					{"index_timestamp", state.TimeStamp }
				};

				if (state.Element != Guid.Empty)
					parameter.Add("element", state.Element);

				itemsParameter.Add(parameter);
			}

			using var w = new Writer("tompit.component_upd_index_state");

			w.CreateParameter("@items", itemsParameter);

			w.Execute();
		}

		public void UpdateStates(List<IComponentAnalyzerState> states)
		{
			var itemsParameter = new JArray();

			foreach (var state in states)
			{
				var parameter = new JObject
				{
					{"id", state.Component.GetId().ToString() },
					{"analyzer_state", (int)state.State },
					{"analyzer_timestamp", state.TimeStamp }
				};

				if (state.Element != Guid.Empty)
					parameter.Add("element", state.Element);

				itemsParameter.Add(parameter);
			}

			using var w = new Writer("tompit.component_upd_analyzer_state");

			w.CreateParameter("@items", itemsParameter);

			w.Execute();
		}

		public List<IComponentDevelopmentState> QueryActiveAnalyzerStates(DateTime timeStamp)
		{
			using var r = new Reader<ComponentState>("tompit.component_state_analyzer_que");

			r.CreateParameter("@timestamp", timeStamp, true);

			return r.Execute().ToList<IComponentDevelopmentState>();
		}

		public List<IComponentDevelopmentState> QueryStates()
		{
			using var r = new Reader<ComponentState>("tompit.component_state_que");

			return r.Execute().ToList<IComponentDevelopmentState>();
		}

		public List<IComponentDevelopmentState> QueryStates(IMicroService microService)
		{
			using var r = new Reader<ComponentState>("tompit.component_state_que");

			r.CreateParameter("@microService", microService.GetId());

			return r.Execute().ToList<IComponentDevelopmentState>();
		}

		public List<IComponentDevelopmentState> QueryStates(IComponent component)
		{
			using var r = new Reader<ComponentState>("tompit.component_state_que");

			r.CreateParameter("@component", component.GetId());

			return r.Execute().ToList<IComponentDevelopmentState>();
		}

		public List<IComponentDevelopmentState> QueryStates(IComponent component, Guid element)
		{
			using var r = new Reader<ComponentState>("tompit.component_state_que");

			r.CreateParameter("@component", component.GetId());
			r.CreateParameter("@element", element);

			return r.Execute().ToList<IComponentDevelopmentState>();
		}

		public List<IComponentDevelopmentState> QueryActiveIndexStates(DateTime timeStamp)
		{
			using var r = new Reader<ComponentState>("tompit.component_state_index_que");

			r.CreateParameter("@timestamp", timeStamp, true);

			return r.Execute().ToList<IComponentDevelopmentState>();
		}
	}
}
