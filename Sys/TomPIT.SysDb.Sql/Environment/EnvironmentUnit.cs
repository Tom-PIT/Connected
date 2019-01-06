using System;
using TomPIT.Data.Sql;
using TomPIT.Environment;

namespace TomPIT.SysDb.Sql.Environment
{
	internal class EnvironmentUnit : PrimaryKeyRecord, IEnvironmentUnit
	{
		public string Name { get; set; }
		public Guid Token { get; set; }
		public Guid Parent { get; set; }
		public int Ordinal { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			Token = GetGuid("token");
			Parent = GetGuid("parent_token");
			Ordinal = GetInt("ordinal");
		}
	}
}
