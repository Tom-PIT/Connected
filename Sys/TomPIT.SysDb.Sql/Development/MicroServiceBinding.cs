using System;
using TomPIT.Data.Sql;
using TomPIT.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class MicroServiceBinding : PrimaryKeyRecord, IMicroServiceBinding
	{
		public long Commit { get; set; }

		public DateTime Date { get; set; }

		public bool Active { get; set; }

		public string RepositoryName { get; set; }

		public string RepositoryUrl { get; set; }

		public DateTime LastCommit { get; set; }
		public Guid Service { get; set; }
		public string ServiceName { get; set; }
		protected override void OnCreate()
		{
			base.OnCreate();

			Service = GetGuid("service_token");
			Commit = GetLong("commit");
			Date = GetDate("date");
			Active = GetBool("active");
			RepositoryName = GetString("repository_name");
			RepositoryUrl = GetString("repository_url");
			LastCommit = GetDate("last_commit");
			ServiceName = GetString("service_name");
		}
	}
}
