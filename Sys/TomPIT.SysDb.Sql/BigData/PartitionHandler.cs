using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.BigData;
using TomPIT.Data.Sql;
using TomPIT.Environment;
using TomPIT.SysDb.BigData;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class PartitionHandler : IPartitionHandler
	{
		public void Delete(IPartition partition)
		{
			var w = new Writer("tompit.big_data_partition_del");

			w.CreateParameter("@id", partition.GetId());

			w.Execute();
		}

		public void DeleteFile(IPartitionFile file)
		{
			var w = new Writer("tompit.big_data_index_del");

			w.CreateParameter("@id", file.GetId());

			w.Execute();
		}

		public void Insert(IResourceGroup resourceGroup, Guid configuration, string name, PartitionStatus status, DateTime created)
		{
			var w = new Writer("tompit.big_data_partition_ins");

			w.CreateParameter("@configuration", configuration);
			w.CreateParameter("@name", name);
			w.CreateParameter("@status", status);
			w.CreateParameter("@created", created);
			w.CreateParameter("@resource_group", resourceGroup.GetId());

			w.Execute();
		}

		public void InsertFile(IPartition partition, INode node, string key, DateTime timestamp, Guid fileToken, PartitionFileStatus status)
		{
			var w = new Writer("tompit.big_data_index_ins");

			w.CreateParameter("@partition", partition.GetId());
			w.CreateParameter("@node", node.GetId());
			w.CreateParameter("@file", fileToken);
			w.CreateParameter("@start_timestamp", timestamp);
			w.CreateParameter("@status", status);
			w.CreateParameter("@key", key, true);

			w.Execute();
		}

		public Guid LockFile(IPartitionFile file)
		{
			var r = new ScalarReader<Guid>("tompit.big_data_index_lock");

			r.CreateParameter("@id", file.GetId());

			return r.ExecuteScalar(Guid.Empty);
		}

		public List<IPartition> Query()
		{
			return new Reader<Partition>("tompit.big_data_partition_que").Execute().ToList<IPartition>();
		}

		public List<IPartitionFieldStatistics> QueryFieldStatistics()
		{
			return new Reader<PartitionFieldStatistics>("tompit.big_data_index_field_sel").Execute().ToList<IPartitionFieldStatistics>();
		}

		public List<IPartitionFile> QueryFiles()
		{
			return new Reader<PartitionFile>("tompit.big_data_index_que").Execute().ToList<IPartitionFile>();
		}

		public IPartition Select(Guid configuration)
		{
			var r = new Reader<Partition>("tompit.big_data_partition_sel");

			r.CreateParameter("@configuration", configuration);

			return r.ExecuteSingleRow();
		}

		public IPartitionFieldStatistics SelectFieldStatistics(IPartitionFile file, string fieldName)
		{
			var r = new Reader<PartitionFieldStatistics>("tompit.big_data_index_field_sel");

			r.CreateParameter("@index", file.GetId());
			r.CreateParameter("@field_name", fieldName);

			return r.ExecuteSingleRow();
		}

		public IPartitionFile SelectFile(Guid fileToken)
		{
			var r = new Reader<PartitionFile>("tompit.big_data_index_sel");

			r.CreateParameter("@file", fileToken);

			return r.ExecuteSingleRow();
		}

		public void UnlockFile(Guid unlockKey)
		{
			var w = new Writer("tompit.big_data_index_unlock");

			w.CreateParameter("@unlock_key", unlockKey);

			w.Execute();
		}

		public void Update(IPartition partition, string name, PartitionStatus status, int fileCount)
		{
			var w = new Writer("tompit.big_data_partition_upd");

			w.CreateParameter("@id", partition.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@status", status);
			w.CreateParameter("@file_count", fileCount);

			w.Execute();
		}

		public void UpdateFieldStatistics(IPartitionFile file, string fieldName, string startString, string endString, double startNumber, double endNumber, DateTime startDate, DateTime endDate)
		{
			var w = new Writer("tompit.big_data_index_field_mdf");

			w.CreateParameter("@index", file.GetId());
			w.CreateParameter("@fieldName", fieldName);
			w.CreateParameter("@start_string", startString, true);
			w.CreateParameter("@end_string", endString, true);
			w.CreateParameter("@start_number", startNumber, true);
			w.CreateParameter("@end_number", endNumber, true);
			w.CreateParameter("@start_date", startDate, true);
			w.CreateParameter("@end_date", endDate, true);

			w.Execute();
		}

		public void UpdateFile(IPartitionFile file, DateTime startTimestamp, DateTime endTimestamp, int count, PartitionFileStatus status)
		{
			var w = new Writer("tompit.big_data_index_upd");

			w.CreateParameter("@id", file.GetId());
			w.CreateParameter("@start_timestamp", startTimestamp, true);
			w.CreateParameter("@end_timestamp", endTimestamp, true);
			w.CreateParameter("@count", count);
			w.CreateParameter("@status", status);

			w.Execute();
		}
	}
}
