using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.BigData;
using TomPIT.Data.Sql;
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

		public void Insert(Guid configuration, string name, PartitionStatus status, DateTime created)
		{
			var w = new Writer("tompit.big_data_partition_ins");

			w.CreateParameter("@configuration", configuration);
			w.CreateParameter("@name", name);
			w.CreateParameter("@status", status);
			w.CreateParameter("@created", created);

			w.Execute();
		}

		public List<IPartition> Query()
		{
			return new Reader<Partition>("tompit.big_data_partition_que").Execute().ToList<IPartition>();
		}

		public IPartition Select(Guid configuration)
		{
			var r = new Reader<Partition>("tompit.big_data_node_sel");

			r.CreateParameter("@configuration", configuration);

			return r.ExecuteSingleRow();

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
	}
}
