using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.BigData;

namespace TomPIT.Management.BigData
{
	public interface IBigDataManagementService
	{
		Guid InsertNode(string name, string connectionString, string adminConnectionString);
		void UpdateNode(Guid token, string name, string connectionString, string adminConnectionString, NodeStatus status, long size);
		void DeleteNode(Guid token);

		List<INode> QueryNodes();
		INode SelectNode(Guid token);
	}
}
