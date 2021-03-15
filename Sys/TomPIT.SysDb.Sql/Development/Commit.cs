using System;
using TomPIT.Data.Sql;
using TomPIT.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class Commit : PrimaryKeyRecord, ICommit
	{
		public DateTime Created { get; set; }
		public Guid User { get; set; }
		public string Comment { get; set; }
		public Guid Service { get; set; }
		public Guid Token { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Created = GetDate("created");
			User = GetGuid("user_token");
			Comment = GetString("comment");
			Service = GetGuid("service");
			Token = GetGuid("token");
		}
	}
}
