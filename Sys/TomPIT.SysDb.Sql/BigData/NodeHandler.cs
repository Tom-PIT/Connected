﻿using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.BigData;
using TomPIT.Data.Sql;
using TomPIT.SysDb.BigData;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class NodeHandler : INodeHandler
	{
		public void Delete(INode node)
		{
			using var w = new Writer("tompit.big_data_node_del");

			w.CreateParameter("@id", node.GetId());

			w.Execute();
		}

		public void Insert(Guid token, string name, string connectionString, string adminConnectionString, NodeStatus status)
		{
			using var w = new Writer("tompit.big_data_node_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@name", name);
			w.CreateParameter("@connection_string", connectionString, true);
			w.CreateParameter("@admin_connection_string", adminConnectionString, true);
			w.CreateParameter("@status", status);

			w.Execute();
		}

		public List<INode> Query()
		{
			using var r = new Reader<Node>("tompit.big_data_node_que");

			return r.Execute().ToList<INode>();
		}

		public INode Select(Guid token)
		{
			using var r = new Reader<Node>("tompit.big_data_node_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public void Update(INode node, string name, string connectionString, string adminConnectionString, NodeStatus status, long size)
		{
			using var w = new Writer("tompit.big_data_node_upd");

			w.CreateParameter("@id", node.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@connection_string", connectionString, true);
			w.CreateParameter("@admin_connection_string", adminConnectionString, true);
			w.CreateParameter("@status", status);
			w.CreateParameter("@size", size);

			w.Execute();
		}
	}
}
