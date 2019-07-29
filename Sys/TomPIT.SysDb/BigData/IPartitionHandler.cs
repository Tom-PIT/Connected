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

		void InsertFile(IPartition partition, INode node, string key, DateTime timestamp, Guid fileToken, PartitionFileStatus status);
		Guid LockFile(IPartitionFile file);
		void UnlockFile(Guid unlockKey);
		void DeleteFile(IPartitionFile file);
		void UpdateFile(IPartitionFile file, DateTime startTimestamp, DateTime endTimestamp, int count, PartitionFileStatus status);
		IPartitionFile SelectFile(Guid fileToken);
		List<IPartitionFile> QueryFiles();

		void UpdateFieldStatistics(IPartitionFile file, string fieldName, string startString, string endString, double startNumber, double endNumber, DateTime startDate, DateTime endDate);
		List<IPartitionFieldStatistics> QueryFieldStatistics();
		IPartitionFieldStatistics SelectFieldStatistics(IPartitionFile file, string fieldName);
	}
}
