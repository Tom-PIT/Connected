using System;
using TomPIT.Data.Sql;
using TomPIT.Quality;

namespace TomPIT.SysDb.Sql.Development
{
	internal class TestSuite : PrimaryKeyRecord, ITestSuite
	{
		public Guid Suite { get; set; }
		public int RunCount { get; set; }
		public int SuccessCount { get; set; }
		public Guid MicroService { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Suite = GetGuid("suite");
			RunCount = GetInt("run_count");
			SuccessCount = GetInt("success_count");
			MicroService = GetGuid("service");
		}
	}
}
