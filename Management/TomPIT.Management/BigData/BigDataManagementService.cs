using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.BigData;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Management.BigData
{
	internal class BigDataManagementService : TenantObject, IBigDataManagementService
	{
		public BigDataManagementService(ITenant tenant) : base(tenant)
		{
		}

		public void DeleteNode(Guid token)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "DeleteNode");
			var e = new JObject
			{
				{"token", token }
			};

			Tenant.Post(u, e);
		}

		public void FixPartition(Guid partition, string name)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "UpdatePartition");
			var e = new JObject
			{
				{"configuration", partition },
				{"name", name },
				{"status", PartitionStatus.Maintenance.ToString() }
			};

			Tenant.Post(u, e);
		}

		public Guid InsertNode(string name, string TenantString, string adminTenantString)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "InsertNode");
			var e = new JObject
			{
				{"name", name },
				{"TenantString", TenantString },
				{"adminTenantString", adminTenantString },
				{"status", NodeStatus.Inactive.ToString() }
			};

			return Tenant.Post<Guid>(u, e);
		}

		public List<IPartitionFile> QueryFiles(Guid partition)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "QueryFilesForPartition");
			var e = new JObject
			{
				{"partition", partition }
			};

			return Tenant.Post<List<PartitionFile>>(u, e).ToList<IPartitionFile>();
		}

		public List<INode> QueryNodes()
		{
			var u = Tenant.CreateUrl("BigDataManagement", "QueryNodes");

			return Tenant.Get<List<Node>>(u).ToList<INode>();
		}

		public List<IPartition> QueryPartitions()
		{
			var u = Tenant.CreateUrl("BigDataManagement", "QueryPartitions");

			return Tenant.Get<List<Partition>>(u).ToList<IPartition>();
		}

		public INode SelectNode(Guid token)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "SelectNode");
			var e = new JObject
			{
				{"token", token }
			};

			return Tenant.Post<Node>(u, e);
		}

		public IPartition SelectPartition(Guid configuration)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "SelectPartition");
			var e = new JObject
			{
				{"configuration", configuration }
			};

			return Tenant.Post<Partition>(u, e);
		}

		public void UpdateNode(Guid token, string name, string TenantString, string adminTenantString, NodeStatus status, long size)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "UpdateNode");
			var e = new JObject
			{
				{"token", token },
				{ "name", name },
				{"TenantString", TenantString },
				{"adminTenantString", adminTenantString },
				{"status", NodeStatus.Inactive.ToString() },
				{"size", size }
			};

			Tenant.Post(u, e);
		}
	}
}
