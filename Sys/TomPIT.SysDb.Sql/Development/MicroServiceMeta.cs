using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Development
{
	internal class MicroServiceMeta : DatabaseRecord
	{
		public string Content { get; set; }

		protected override void OnCreate()
		{
			Content = GetString("meta");
		}
	}
}
