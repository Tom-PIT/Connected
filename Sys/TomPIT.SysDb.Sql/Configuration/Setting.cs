using System;
using TomPIT.Configuration;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Configuration
{
	internal class Setting : PrimaryKeyRecord, ISetting
	{
		public string Name { get; set; }
		public string Value { get; set; }
		public bool Visible { get; set; }
		public DataType DataType { get; set; }
		public string Tags { get; set; }
		public Guid ResourceGroup { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			Value = GetString("value");
			Visible = GetBool("visible");
			DataType = GetValue("data_type", DataType.String);
			Tags = GetString("tags");
			ResourceGroup = GetGuid("resource_token");
		}
	}
}
