using System;

namespace TomPIT.QA
{
	public interface ITestSessionScenario : ITestSessionEntity
	{
		Guid Session { get; }
		Guid Scenario { get; }

	}
}
