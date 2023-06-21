using System;
using TomPIT.BigData;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class PartitionBuffer : PrimaryKeyRecord, IPartitionBuffer
	{
		public Guid Partition { get; set; }

		public DateTime NextVisible { get; set; }
		 
      protected override void OnCreate()
		{
			base.OnCreate();

			Partition = GetGuid("partition");
			NextVisible = GetDate("next_visible");
		}
	}
}
