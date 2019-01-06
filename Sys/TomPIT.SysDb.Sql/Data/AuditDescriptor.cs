using System;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Data
{
	internal class AuditDescriptor : LongPrimaryKeyRecord, TomPIT.Data.IAuditDescriptor
	{
		public Guid User { get; set; }
		public DateTime Created { get; set; }
		public string PrimaryKey { get; set; }
		public string Category { get; set; }
		public string Event { get; set; }
		public string Description { get; set; }
		public string Ip { get; set; }
		public string Property { get; set; }
		public string Value { get; set; }
		public Guid Identifier { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			User = GetGuid("user");
			Created = GetDate("created");
			PrimaryKey = GetString("primary_key");
			Category = GetString("category");
			Event = GetString("event");
			Description = GetString("description");
			Ip = GetString("ip");
			Property = GetString("property");
			Value = GetString("value");
			Identifier = GetGuid("identifier");
		}
	}
}
