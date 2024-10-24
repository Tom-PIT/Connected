﻿using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data.Sql;
using TomPIT.Environment;
using TomPIT.SysDb.Environment;

namespace TomPIT.SysDb.Sql.Environment
{
	internal class ClientHandler : IClientHandler
	{
		public void Delete(IClient client)
		{
			using var w = new Writer("tompit.client_del");

			w.CreateParameter("@id", client.GetId());

			w.Execute();
		}

		public void Insert(string token, string name, DateTime created, ClientStatus status, string type)
		{
			using var w = new Writer("tompit.client_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@name", name);
			w.CreateParameter("@created", created);
			w.CreateParameter("@status", status);
			w.CreateParameter("@type", type, true);

			w.Execute();
		}

		public List<IClient> Query()
		{
			using var r = new Reader<Client>("tompit.client_que");

			return r.Execute().ToList<IClient>();
		}

		public IClient Select(string token)
		{
			using var r = new Reader<Client>("tompit.client_sel");

			r.CreateParameter("token", token);

			return r.ExecuteSingleRow();
		}

		public void Update(IClient client, string name, ClientStatus status, string type)
		{
			using var w = new Writer("tompit.client_upd");

			w.CreateParameter("@id", client.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@status", status);
			w.CreateParameter("@type", type, true);

			w.Execute();
		}
	}
}
