using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.BigData;
using TomPIT.Data.Sql;
using TomPIT.SysDb.BigData;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class PartitionBufferHandler : IPartitionBufferHandler
	{
		public List<IPartitionBuffer> Query()
		{
			using var r = new Reader<PartitionBuffer>("tompit.big_data_buffer_que");

			return r.Execute().ToList<IPartitionBuffer>();
		}

		public void Update(List<IPartitionBuffer> buffers)
		{
			using var w = new Writer("tompit.big_data_buffer_mdf");
			var a = new JArray();

			foreach(var item in buffers)
			{
				a.Add(new JObject
				{
					{"id", item.Id },
					{"partition", item.Partition },
					{"next_visible", item.NextVisible }
				});
			}
			
			w.CreateParameter("@items", a);

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
