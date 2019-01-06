using System;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Development
{
	internal class Component : PrimaryKeyRecord, IComponent
	{
		public string Name { get; set; }
		public Guid MicroService { get; set; }
		public Guid Feature { get; set; }
		public Guid Token { get; set; }
		public string Type { get; set; }
		public string Category { get; set; }
		public Guid RuntimeConfiguration { get; set; }
		public DateTime Modified { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			MicroService = GetGuid("service_token");
			Feature = GetGuid("feature_token");
			Token = GetGuid("token");
			Type = GetString("type");
			Category = GetString("category");
			RuntimeConfiguration = GetGuid("runtime_configuration");
			Modified = GetDate("modified");
		}
	}
}
