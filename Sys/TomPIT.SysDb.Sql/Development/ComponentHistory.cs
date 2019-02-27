using System;
using TomPIT.Data.Sql;
using TomPIT.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class ComponentHistory : PrimaryKeyRecord, IComponentHistory
	{
		public DateTime Created { get; set; }
		public Guid Blob { get; set; }
		public string Name { get; set; }
		public Guid User { get; set; }
		public Guid Commit { get; set; }
		public Guid Component { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Created = GetDate("created");
			Blob = GetGuid("configuration");
			Name = GetString("name");
			User = GetGuid("user_token");
			Commit = GetGuid("commit_token");
			Component = GetGuid("component");
		}
	}
}
