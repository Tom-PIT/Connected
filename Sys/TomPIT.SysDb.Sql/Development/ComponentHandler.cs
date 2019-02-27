using System;
using System.Collections.Generic;
using System.Linq;
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
			var w = new Writer("tompit.component_del");

			w.CreateParameter("@id", component.GetId());

			w.Execute();
		}

		public void Insert(IMicroService service, DateTime modified, IFolder folder, string category, string name, Guid token, string type, Guid runtimeConfiguration)
		{
			var w = new Writer("tompit.component_ins");

			w.CreateParameter("@folder", folder == null ? 0 : folder.GetId(), true);
			w.CreateParameter("@name", name);
			w.CreateParameter("@token", token);
			w.CreateParameter("@type", type);
			w.CreateParameter("@category", category);
			w.CreateParameter("@runtime_configuration", runtimeConfiguration, true);
			w.CreateParameter("@service", service.GetId());
			w.CreateParameter("@modified", modified);

			w.Execute();
		}

		public List<IComponent> Query()
		{
			return new Reader<Component>("tompit.component_que").Execute().ToList<IComponent>();
		}

		public IComponent Select(Guid component)
		{
			var r = new Reader<Component>("tompit.component_sel");

			r.CreateParameter("@component", component);

			return r.ExecuteSingleRow();
		}

		public List<IComponent> Query(string category, string name)
		{
			var r = new Reader<Component>("tompit.component_que");

			r.CreateParameter("@name", name);
			r.CreateParameter("@category", category);

			return r.Execute().ToList<IComponent>();
		}

		public IComponent Select(IMicroService microService, string category, string name)
		{
			var r = new Reader<Component>("tompit.component_sel");

			r.CreateParameter("@service", microService.GetId());
			r.CreateParameter("@name", name);
			r.CreateParameter("@category", category);

			return r.ExecuteSingleRow();
		}

		public void Update(IComponent component, DateTime modified, string name, IFolder folder, Guid runtimeConfiguration)
		{
			var w = new Writer("tompit.component_upd");

			w.CreateParameter("@id", component.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@modified", modified);
			w.CreateParameter("@folder", folder == null ? 0 : folder.GetId(), true);
			w.CreateParameter("@runtime_configuration", runtimeConfiguration, true);

			w.Execute();
		}

		public void Update(IComponent component, IUser user, LockStatus status, LockVerb verb, DateTime date)
		{
			var w = new Writer("tompit.component_lock_upd");

			w.CreateParameter("@id", component.GetId());
			w.CreateParameter("@lock_status", status);
			w.CreateParameter("@lock_user", user == null ? 0 : user.GetId(), true);
			w.CreateParameter("@lock_date", date, true);
			w.CreateParameter("@lock_verb", verb);

			w.Execute();
		}
	}
}
