using System;

namespace TomPIT.QA
{
	public interface ITestSessionTestCase : ITestSessionEntity
	{
		Guid Scenario { get; }
		Guid TestCase { get; }
	}
}
