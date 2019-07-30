using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.BigData;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Management.BigData
{
	internal class BigDataManagementService : ServiceBase, IBigDataManagementService
	{
		public BigDataManagementService(ISysConnection connection) : base(connection)
		{
		}

		public void DeleteNode(Guid token)
		{
			var u = Connection.CreateUrl("BigDataManagement", "DeleteNode");
			var e = new JObject
			{
				{"token", token }
			};

			Connection.Post(u, e);
		}

		public void FixPartition(Guid partition, string name)
		{
			var u = Connection.CreateUrl("BigDataManagement", "UpdatePartition");
			var e = new JObject
			{
				{"configuration", partition },
				{"name", name },
				{"status", PartitionStatus.Maintenance.ToString() }
			};

			Connection.Post(u, e);
		}

		public Guid InsertNode(string name, string connectionString, string adminConnectionString)
		{
			var u = Connection.CreateUrl("BigDataManagement", "InsertNode");
			var e = new JObject
			{
				{"name", name },
				{"connectionString", connectionString },
				{"adminConnectionString", adminConnectionString },
				{"status", NodeStatus.Inactive.ToString() }
			};

			return Connection.Post<Guid>(u, e);
		}

		public List<IPartitionFile> QueryFiles(Guid partition)
		{
			var u = Connection.CreateUrl("BigDataManagement", "QueryFilesForPartition");
			var e = new JObject
			{
				{"partition", partition }
			};

			return Connection.Post<List<PartitionFile>>(u, e).ToList<IPartitionFile>();
		}

		public List<INode> QueryNodes()
		{
			var u = Connection.CreateUrl("BigDataManagement", "QueryNodes");

			return Connection.Get<List<Node>>(u).ToList<INode>();
		}

		public List<IPartition> QueryPartitions()
		{
			var u = Connection.CreateUrl("BigDataManagement", "QueryPartitions");

			return Connection.Get<List<Partition>>(u).ToList<IPartition>();
		}

		public INode SelectNode(Guid token)
		{
			var u = Connection.CreateUrl("BigDataManagement", "SelectNode");
			var e = new JObject
			{
				{"token", token }
			};

			return Connection.Post<Node>(u, e);
		}

		public IPartition SelectPartition(Guid configuration)
		{
			var u = Connection.CreateUrl("BigDataManagement", "SelectPartition");
			var e = new JObject
			{
				{"configuration", configuration }
			};

			return Connection.Post<Partition>(u, e);
		}

		public void UpdateNode(Guid token, string name, string connectionString, string adminConnectionString, NodeStatus status, long size)
		{
			var u = Connection.CreateUrl("BigDataManagement", "UpdateNode");
			var e = new JObject
			{
				{"token", token },
				{ "name", name },
				{"connectionString", connectionString },
				{"adminConnectionString", adminConnectionString },
				{"status", NodeStatus.Inactive.ToString() },
				{"size", size }
			};

			Connection.Post(u, e);
		}
	}
}
