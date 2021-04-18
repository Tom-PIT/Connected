using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.BigData.Persistence;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.BigData.Partitions
{
	public interface IPartitionService
	{
		ImmutableList<IPartition> Query();
		IPartition Select(IPartitionConfiguration configuration);
		IPartitionFile SelectFile(Guid fileName);
		ImmutableList<IPartitionFile> QueryFiles(Guid partition, string key, DateTime startTimestamp, DateTime endTimestamp, List<IndexParameter> parameters);
		ImmutableList<IPartitionFile> QueryFiles(Guid partition, string key, DateTime startTimestamp, DateTime endTimestamp);
		ImmutableList<IPartitionFile> QueryFiles(Guid partition);
		Guid InsertFile(Guid partition, Guid node, string key, DateTime timeStamp);
		void UpdateFile(Guid file, DateTime startTimeStamp, DateTime endTimeStamp, int count, PartitionFileStatus status);
		void UpdatePartition(Guid token, string name, PartitionStatus status);
		void UpdateFileStatistics(Guid file, string fieldName, string startString, string endString, double startNumber, double endNumber, DateTime startDate, DateTime endDate);
		void DeleteFile(Guid file);
		Guid LockFile(Guid file);
		void ReleaseFile(Guid unlockKey);
		void NotifyChanged(Guid token);
		void NotifyRemoved(Guid token);

		void NotifyFileChanged(Guid token);
		void NotifyFileRemoved(Guid token);
		void NotifyFieldStatisticChanged(Guid file, string fieldName);

		void ValidateSchema(Guid partition);
		void SaveSchemaImage(Guid partition);
	}
}
