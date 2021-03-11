using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data.Sql;
using TomPIT.Security;
using TomPIT.SysDb.Security;

namespace TomPIT.SysDb.Sql.Security
{
	internal class RoleHandler : IRoleHandler
	{
		public void Delete(IRole role)
		{
			using var w = new Writer("tompit.role_del");

			w.CreateParameter("@id", role.GetId());

			w.Execute();
		}

		public void Insert(Guid token, string name)
		{
			using var w = new Writer("tompit.role_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@name", name);

			w.Execute();
		}

		public List<IRole> Query()
		{
			using var r = new Reader<Role>("tompit.role_que");

			return r.Execute().ToList<IRole>();
		}

		public IRole Select(Guid token)
		{
			using var r = new Reader<Role>("tompit.role_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public void Update(IRole role, string name)
		{
			using var w = new Writer("tompit.role_upd");

			w.CreateParameter("@id", role.GetId());
			w.CreateParameter("@name", name);

			w.Execute();
		}
	}
}
