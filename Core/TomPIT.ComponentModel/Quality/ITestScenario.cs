using TomPIT.Collections;

namespace TomPIT.ComponentModel.Quality
{
	public interface ITestScenario : ITestElement
	{
		ListItems<ITestCase> TestCases { get; }
	}
}
