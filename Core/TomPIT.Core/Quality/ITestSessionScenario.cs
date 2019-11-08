using System;

namespace TomPIT.Quality
{
	public interface ITestSessionScenario : ITestSessionEntity
	{
		Guid Session { get; }
		Guid Scenario { get; }

	}
}
