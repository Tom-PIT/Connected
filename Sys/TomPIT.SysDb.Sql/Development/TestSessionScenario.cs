using System;
using TomPIT.Quality;

namespace TomPIT.SysDb.Sql.Development
{
	internal class TestSessionScenario : TestSessionEntity, ITestSessionScenario
	{
		public Guid Session { get; set; }
		public Guid Scenario { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Session = GetGuid("session_token");
			Scenario = GetGuid("scenario");
		}
	}
}
