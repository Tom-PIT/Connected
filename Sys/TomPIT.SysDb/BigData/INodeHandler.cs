using System;
using System.Collections.Generic;
using TomPIT.BigData;

namespace TomPIT.SysDb.BigData
{
	public interface INodeHandler
	{
		INode Select(Guid token);
		List<INode> Query();
		void Insert(Guid token, string name, string connectionString, string adminConnectionString, NodeStatus status);
		void Update(INode node, string name, string connectionString, string adminConnectionString, NodeStatus status, long size);
		void Delete(INode node);
	}
}
