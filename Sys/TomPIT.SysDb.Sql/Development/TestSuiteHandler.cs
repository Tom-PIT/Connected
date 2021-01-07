using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data.Sql;
using TomPIT.Quality;
using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class TestSuiteHandler : ITestSuiteHandler
	{
		public void DeleteSuite(ITestSuite suite)
		{
			using var w = new Writer("tompit.test_suite_del");

			w.CreateParameter("@suite", suite.GetId());

			w.Execute();
		}

		public void InsertSession(ITestSuite suite, Guid token, DateTime start)
		{
			using var w = new Writer("tompit.test_session_ins");

			w.CreateParameter("@suite", suite.GetId());
			w.CreateParameter("@start", start);
			w.CreateParameter("@token", token);

			w.Execute();
		}

		public void InsertSessionScenario(ITestSession session, Guid scenario, DateTime start)
		{
			using var w = new Writer("tompit.test_session_scenario_ins");

			w.CreateParameter("@session", session.GetId());
			w.CreateParameter("@start", start);
			w.CreateParameter("@scenario", scenario);

			w.Execute();
		}

		public void InsertSessionTestCase(ITestSessionScenario scenario, Guid testCase, DateTime start)
		{
			using var w = new Writer("tompit.test_session_case_ins");

			w.CreateParameter("@scenario", scenario.GetId());
			w.CreateParameter("@start", start);
			w.CreateParameter("@test_case", testCase);

			w.Execute();
		}

		public List<ITestSessionScenario> QueryScenarios(ITestSession session)
		{
			using var r = new Reader<TestSessionScenario>("tompit.test_session_scenario_que");

			r.CreateParameter("@session", session.GetId());

			return r.Execute().ToList<ITestSessionScenario>();
		}

		public List<ITestSession> QuerySessions(ITestSuite suite, bool includeCompleted)
		{
			using var r = new Reader<TestSession>("tompit.test_session_que");

			r.CreateParameter("@suite", suite.GetId());

			return r.Execute().ToList<ITestSession>();
		}

		public List<ITestSessionTestCase> QueryTestCases(ITestSessionScenario scenario)
		{
			using var r = new Reader<TestSessionTestCase>("tompit.test_session_case_que");

			r.CreateParameter("@scenario", scenario.GetId());

			return r.Execute().ToList<ITestSessionTestCase>();
		}

		public ITestSession SelectLastSession(ITestSuite suite)
		{
			using var r = new Reader<TestSession>("tompit.test_session_sel_last");

			r.CreateParameter("@suite", suite.GetId());

			return r.ExecuteSingleRow();
		}

		public ITestSessionScenario SelectScenario(ITestSession session, Guid scenario)
		{
			using var r = new Reader<TestSessionScenario>("tompit.test_session_scenario_sel");

			r.CreateParameter("@session", session.GetId());
			r.CreateParameter("@scenario", scenario);

			return r.ExecuteSingleRow();
		}

		public ITestSession SelectSession(Guid token)
		{
			using var r = new Reader<TestSession>("tompit.test_session_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public ITestSuite SelectSuite(Guid suite)
		{
			using var r = new Reader<TestSuite>("tompit.test_suite_sel");

			r.CreateParameter("@suite", suite);

			return r.ExecuteSingleRow();
		}

		public ITestSessionTestCase SelectTestCase(ITestSessionScenario scenario, Guid testCase)
		{
			using var r = new Reader<TestSessionTestCase>("tompit.test_session_case_sel");

			r.CreateParameter("@scenario", scenario.GetId());
			r.CreateParameter("@test_case", testCase);

			return r.ExecuteSingleRow();
		}

		public void UpdateSession(ITestSession session, TestRunStatus status, TestRunResult result, DateTime complete, string error)
		{
			using var w = new Reader<TestSessionTestCase>("tompit.test_session_upd");

			w.CreateParameter("@id", session.GetId());
			w.CreateParameter("@status", status);
			w.CreateParameter("@result", result);
			w.CreateParameter("@complete", complete, true);
			w.CreateParameter("@error", error, true);

			w.Execute();
		}

		public void UpdateSessionScenario(ITestSessionScenario scenario, TestRunStatus status, TestRunResult result, DateTime complete, string error)
		{
			using var w = new Reader<TestSessionTestCase>("tompit.test_session_scenario_upd");

			w.CreateParameter("@id", scenario.GetId());
			w.CreateParameter("@status", status);
			w.CreateParameter("@result", result);
			w.CreateParameter("@complete", complete, true);
			w.CreateParameter("@error", error, true);

			w.Execute();
		}

		public void UpdateSessionTestCase(ITestSessionTestCase testCase, TestRunStatus status, TestRunResult result, DateTime complete, string error)
		{
			using var w = new Reader<TestSessionTestCase>("tompit.test_session_case_upd");

			w.CreateParameter("@id", testCase.GetId());
			w.CreateParameter("@status", status);
			w.CreateParameter("@result", result);
			w.CreateParameter("@complete", complete, true);
			w.CreateParameter("@error", error, true);

			w.Execute();
		}
	}
}
