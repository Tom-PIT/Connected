using System;
using TomPIT.Quality;

namespace TomPIT.SysDb.Sql.Development
{
	internal class TestSessionTestCase : TestSessionEntity, ITestSessionTestCase
	{
		public Guid Scenario { get; set; }
		public Guid TestCase { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Scenario = GetGuid("scenario_token");
			TestCase = GetGuid("test_case");
		}
	}
}
