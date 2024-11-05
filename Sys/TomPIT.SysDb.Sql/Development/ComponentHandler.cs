using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;
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

		public void Insert(IMicroService service, DateTime modified, IFolder folder, string category, string nameSpace, string name, Guid token, string type)
		{
			using var w = new Writer("tompit.component_ins");

			w.CreateParameter("@folder", folder == null ? 0 : folder.GetId(), true);
			w.CreateParameter("@name", name);
			w.CreateParameter("@token", token);
			w.CreateParameter("@type", type);
			w.CreateParameter("@category", category);
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

		public void Update(IComponent component, DateTime modified, string name, IFolder folder)
		{
			using var w = new Writer("tompit.component_upd");

			w.CreateParameter("@id", component.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@modified", modified);
			w.CreateParameter("@folder", folder == null ? 0 : folder.GetId(), true);

			w.Execute();
		}
	}
}
