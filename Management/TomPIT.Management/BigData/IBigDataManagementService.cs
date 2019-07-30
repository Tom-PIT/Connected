using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.BigData;

namespace TomPIT.Management.BigData
{
	public interface IBigDataManagementService
	{
		List<IPartition> QueryPartitions();
		IPartition SelectPartition(Guid configuration);
		void FixPartition(Guid partition, string name);
		Guid InsertNode(string name, string connectionString, string adminConnectionString);
		void UpdateNode(Guid token, string name, string connectionString, string adminConnectionString, NodeStatus status, long size);
		void DeleteNode(Guid token);

		List<INode> QueryNodes();
		List<IPartitionFile> QueryFiles(Guid partition);
		INode SelectNode(Guid token);
	}
}
