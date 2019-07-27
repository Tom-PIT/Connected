using System;
using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.Environment;

namespace TomPIT.SysDb.BigData
{
	public interface IPartitionHandler
	{
		List<IPartition> Query();
		IPartition Select(Guid configuration);

		void Insert(IResourceGroup resourceGroup, Guid configuration, string name, PartitionStatus status, DateTime created);
		void Update(IPartition partition, string name, PartitionStatus status, int fileCount);
		void Delete(IPartition partition);
	}
}
