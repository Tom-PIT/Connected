using System;
using TomPIT.BigData;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class Node : PrimaryKeyRecord, INode
	{
		public string Name { get; set; }
		public string ConnectionString { get; set; }
		public string AdminConnectionString { get; set; }
		public Guid Token { get; set; }
		public NodeStatus Status { get; set; }
		public long Size { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			ConnectionString = GetString("connection_string");
			AdminConnectionString = GetString("admin_connection_string");
			Token = GetGuid("token");
			Status = GetValue("status", NodeStatus.Inactive);
			Size = GetLong("size");
		}
	}
}
