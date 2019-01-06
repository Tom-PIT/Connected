using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Development
{
	internal class MicroServiceMeta : DatabaseRecord
	{
		public byte[] Content { get; set; }

		protected override void OnCreate()
		{
			Content = GetValue<byte[]>("meta", null);
		}
	}
}
