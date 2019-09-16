using TomPIT.Collections;

namespace TomPIT.ComponentModel.Quality
{
	public enum TestErrorBehavior
	{
		Stop = 1,
		Continue = 2
	}

	public interface ITestSuiteConfiguration : IConfiguration
	{
		ListItems<ITestScenario> Scenarios { get; }
	}
}
