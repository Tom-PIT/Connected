using System;
using TomPIT.Analytics;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Analytics
{
	internal class Mru : LongPrimaryKeyRecord, IMru
	{
		public Guid Token { get; set; }

		public int Type { get; set; }

		public string PrimaryKey { get; set; }

		public AnalyticsEntity Entity { get; set; }

		public string EntityPrimaryKey { get; set; }

		public DateTime Date { get; set; }

		public string Tag { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Token = GetGuid("token");
			Type = GetInt("type");
			PrimaryKey = GetString("primary_key");
			Entity = GetValue("entity_type", AnalyticsEntity.User);
			EntityPrimaryKey = GetString("entity_primary_key");
			Date = GetDate("date");
			Tag = GetString("tag");
		}
	}
}
