using System;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Development
{
	internal class MicroServiceString : PrimaryKeyRecord, IMicroServiceString
	{
		public Guid MicroService { get; set; }
		public Guid Element { get; set; }
		public string Property { get; set; }
		public string Value { get; set; }
		public Guid Language { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			MicroService = GetGuid("service_token");
			Element = GetGuid("element");
			Property = GetString("property");
			Value = GetString("value");
			Language = GetGuid("language_token");
		}
	}
}
