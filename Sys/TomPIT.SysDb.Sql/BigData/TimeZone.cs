using System;
using TomPIT.BigData;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.BigData
{
	internal class TimeZone : PrimaryKeyRecord, ITimeZone
	{
		public string Name { get; set; }

		public int Offset { get; set; }

		public Guid Token { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			Offset = GetInt("offset");
			Token = GetGuid("token");
		}
	}
}
