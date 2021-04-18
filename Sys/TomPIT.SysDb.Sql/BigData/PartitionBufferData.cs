using TomPIT.BigData;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.BigData
{
	public class PartitionBufferData : LongPrimaryKeyRecord, IPartitionBufferData
	{
		public byte[] Data { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Data = GetValue<byte[]>("data", null);
		}
	}
}
