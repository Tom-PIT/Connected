using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.BigData;
using TomPIT.Data.Sql;
using TomPIT.SysDb.BigData;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class PartitionBufferHandler : IPartitionBufferHandler
	{
		public List<IPartitionBuffer> Dequeue(int count, DateTime date, DateTime nextVisible)
		{
			using var r = new Reader<PartitionBuffer>("tompit.big_data_buffer_dequeue");

			r.CreateParameter("@next_visible", nextVisible);
			r.CreateParameter("@count", count);
			r.CreateParameter("@date", date);

			return r.Execute().ToList<IPartitionBuffer>();
		}

		public List<IPartitionBuffer> Query()
		{
			using var r = new Reader<PartitionBuffer>("tompit.big_data_buffer_que");

			return r.Execute().ToList<IPartitionBuffer>();
		}

		public IPartitionBuffer Select(Guid partition)
		{
			using var r = new Reader<PartitionBuffer>("tompit.big_data_buffer_sel");

			r.CreateParameter("@partition", partition);

			return r.ExecuteSingleRow();
		}

		public void Insert(Guid partition, DateTime nextVisible)
		{
			using var w = new Writer("tompit.big_data_buffer_ins");

			w.CreateParameter("@partition", partition);
			w.CreateParameter("@next_visible", nextVisible);

			w.Execute();
		}

		public void Update(IPartitionBuffer buffer, DateTime nextVisibe)
		{
			using var w = new Writer("tompit.big_data_buffer_upd");

			w.CreateParameter("@id", buffer.GetId());
			w.CreateParameter("@next_visible", nextVisibe);

			w.Execute();
		}

		public List<IPartitionBufferData> QueryData(IPartitionBuffer buffer)
		{
			using var r = new Reader<PartitionBufferData>("tompit.big_data_buffer_data_que");

			r.CreateParameter("@buffer", buffer.GetId());

			return r.Execute().ToList<IPartitionBufferData>();
		}

		public void Clear(IPartitionBuffer buffer, long id)
		{
			using var w = new Writer("tompit.big_data_buffer_data_clr");

			w.CreateParameter("@buffer", buffer.GetId());
			w.CreateParameter("@id", id);

			w.Execute();
		}

		public void InsertData(IPartitionBuffer buffer, byte[] data)
		{
			using var w = new Writer("tompit.big_data_buffer_data_ins");

			w.CreateParameter("@buffer", buffer.GetId());
			w.CreateParameter("@data", data);

			w.Execute();
		}
	}
}
