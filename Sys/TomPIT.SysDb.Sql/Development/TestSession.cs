using System;
using TomPIT.QA;

namespace TomPIT.SysDb.Sql.Development
{
	internal class TestSession : TestSessionEntity, ITestSession
	{
		public Guid Suite { get; set; }
		public Guid Token { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Suite = GetGuid("suite_token");
			Token = GetGuid("token");
		}
	}
}
