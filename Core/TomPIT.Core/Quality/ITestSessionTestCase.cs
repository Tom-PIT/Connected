using System;

namespace TomPIT.Quality
{
	public interface ITestSessionTestCase : ITestSessionEntity
	{
		Guid Scenario { get; }
		Guid TestCase { get; }
	}
}
