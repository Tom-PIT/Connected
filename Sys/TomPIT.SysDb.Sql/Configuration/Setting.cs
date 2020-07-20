using TomPIT.Configuration;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Configuration
{
	internal class Setting : PrimaryKeyRecord, ISetting
	{
		public string Name { get; set; }
		public string Value { get; set; }
		public string Type { get; set; }
		public string PrimaryKey { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			Value = GetString("value");
			Type = GetString("type");
			PrimaryKey = GetString("primary_key");
		}
	}
}
