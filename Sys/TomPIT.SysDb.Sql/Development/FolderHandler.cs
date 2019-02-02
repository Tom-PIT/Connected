using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class FolderHandler : IFolderHandler
	{
		public void Delete(IFolder folder)
		{
			var w = new Writer("tompit.folder_del");

			w.CreateParameter("@id", folder.GetId());

			w.Execute();
		}

		public void Insert(IMicroService service, string name, Guid token, IFolder parent)
		{
			var w = new Writer("tompit.folder_ins");

			w.CreateParameter("@service", service.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@token", token);
			w.CreateParameter("@parent", parent == null ? 0 : parent.GetId(), true);

			w.Execute();
		}

		public List<IFolder> Query()
		{
			return new Reader<Folder>("tompit.folder_que").Execute().ToList<IFolder>();
		}

		public IFolder Select(Guid token)
		{
			var r = new Reader<Folder>("tompit.folder_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public IFolder Select(IMicroService microService, string name)
		{
			var r = new Reader<Folder>("tompit.folder_sel");

			r.CreateParameter("@service", microService.GetId());
			r.CreateParameter("@name", name);

			return r.ExecuteSingleRow();
		}

		public void Update(IFolder folder, string name, IFolder parent)
		{
			var w = new Writer("tompit.folder_upd");

			w.CreateParameter("@id", folder.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@parent", parent == null ? 0 : parent.GetId(), true);

			w.Execute();
		}
	}
}
