using System;
using System.Collections.Generic;
using TomPIT.Quality;

namespace TomPIT.SysDb.Development
{
	public interface ITestSuiteHandler
	{
		void InsertSession(ITestSuite suite, Guid token, DateTime start);
		void UpdateSession(ITestSession session, TestRunStatus status, TestRunResult result, DateTime complete, string error);

		void InsertSessionScenario(ITestSession session, Guid scenario, DateTime start);
		void UpdateSessionScenario(ITestSessionScenario scenario, TestRunStatus status, TestRunResult result, DateTime complete, string error);

		void InsertSessionTestCase(ITestSessionScenario scenario, Guid testCase, DateTime start);
		void UpdateSessionTestCase(ITestSessionTestCase testCase, TestRunStatus status, TestRunResult result, DateTime complete, string error);

		void DeleteSuite(ITestSuite suite);

		ITestSuite SelectSuite(Guid suite);
		List<ITestSession> QuerySessions(ITestSuite suite, bool includeCompleted);
		ITestSession SelectLastSession(ITestSuite suite);
		ITestSession SelectSession(Guid token);

		ITestSessionScenario SelectScenario(ITestSession session, Guid scenario);
		List<ITestSessionScenario> QueryScenarios(ITestSession session);

		ITestSessionTestCase SelectTestCase(ITestSessionScenario scenario, Guid testCase);
		List<ITestSessionTestCase> QueryTestCases(ITestSessionScenario scenario);
	}
}
