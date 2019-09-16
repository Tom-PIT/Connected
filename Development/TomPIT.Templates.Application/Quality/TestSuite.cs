using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Quality;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Quality
{
	public class TestSuite : ComponentConfiguration, ITestSuiteConfiguration
	{
		private ListItems<ITestScenario> _scenarios = null;

		[Items(DesignUtils.ScenariosItems)]
		public ListItems<ITestScenario> Scenarios
		{
			get
			{
				if (_scenarios == null)
					_scenarios = new ListItems<ITestScenario> { Parent = this };

				return _scenarios;
			}
		}
	}
}
