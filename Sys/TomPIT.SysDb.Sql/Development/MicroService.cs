using System;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Development
{
	internal class MicroService : PrimaryKeyRecord, IMicroService
	{
		public string Name { get; set; }
		public string Url { get; set; }
		public Guid Token { get; set; }
		public MicroServiceStatus Status { get; set; }
		public Guid ResourceGroup { get; set; }
		public Guid Template { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			Url = GetString("url");
			Token = GetGuid("token");
			Status = GetValue("status", MicroServiceStatus.Development);
			ResourceGroup = GetGuid("resource_token");
			Template = GetGuid("template");
		}
	}
}
