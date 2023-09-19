using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.BigData;
using TomPIT.Connectivity;

namespace TomPIT.Management.BigData
{
	internal class BigDataManagementService : TenantObject, IBigDataManagementService
	{
		public BigDataManagementService(ITenant tenant) : base(tenant)
		{
		}

		public void DeleteNode(Guid token)
		{
			Instance.SysProxy.Management.BigData.DeleteNode(token);
		}

		public void FixPartition(Guid partition, string name)
		{
			Instance.SysProxy.Management.BigData.UpdatePartition(partition, name, PartitionStatus.Maintenance);
		}

		public Guid InsertNode(string name, string connectionString, string adminConnectionString)
		{
			return Instance.SysProxy.Management.BigData.InsertNode(name, connectionString, adminConnectionString, NodeStatus.Inactive);
		}

		public List<IPartitionFile> QueryFiles(Guid partition)
		{
			return Instance.SysProxy.Management.BigData.QueryFilesForPartition(partition).ToList();
		}

		public List<INode> QueryNodes()
		{
			return Instance.SysProxy.Management.BigData.QueryNodes().ToList();
		}

		public List<IPartition> QueryPartitions()
		{
			return Instance.SysProxy.Management.BigData.QueryPartitions().ToList();
		}

		public INode SelectNode(Guid token)
		{
			return Instance.SysProxy.Management.BigData.SelectNode(token);
		}

		public IPartition SelectPartition(Guid configuration)
		{
			return Instance.SysProxy.Management.BigData.SelectPartition(configuration);
		}

		public void UpdateNode(Guid token, string name, string connectionString, string adminConnectionString, NodeStatus status, long size)
		{
			Instance.SysProxy.Management.BigData.UpdateNode(token, name, connectionString, adminConnectionString, status, size);
		}
	}
}
