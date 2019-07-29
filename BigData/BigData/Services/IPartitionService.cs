using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.BigData.Services
{
	public interface IPartitionService
	{
		List<IPartition> Query();
		IPartition Select(IPartitionConfiguration configuration);
		IPartitionFile SelectFile(Guid fileName);
		List<IPartitionFile> QueryFiles(Guid partition, string key, DateTime startTimestamp, DateTime endTimestamp);
		Guid InsertFile(Guid partition, Guid node, string key, DateTime timeStamp);
		void UpdateFile(Guid file, DateTime startTimeStamp, DateTime endTimeStamp, int count, PartitionFileStatus status);
		void UpdateFileStatistics(Guid file, string fieldName, string startString, string endString, double startNumber, double endNumber, DateTime startDate, DateTime endDate);
		void DeleteFile(Guid file);
		Guid LockFile(Guid file);
		void ReleaseFile(Guid unlockKey);
		void NotifyChanged(Guid token);
		void NotifyRemoved(Guid token);

		void NotifyFileChanged(Guid token);
		void NotifyFileRemoved(Guid token);
		void NotifyFieldStatisticChanged(Guid file, string fieldName);
	}
}
