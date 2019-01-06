using System;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Features;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Development
{
	internal class Feature : PrimaryKeyRecord, IFeature
	{
		public string Name { get; set; }

		public Guid Token { get; set; }

		public Guid MicroService { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			Token = GetGuid("token");
			MicroService = GetGuid("service_token");
		}
	}
}
