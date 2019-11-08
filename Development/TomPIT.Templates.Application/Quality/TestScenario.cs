using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel.Quality;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Quality
{
	[Create(DesignUtils.TestScenario, nameof(Name))]
	public class TestScenario : TestElement, ITestScenario
	{
		private ListItems<ITestCase> _testCases = null;

		[Items(DesignUtils.TestCasesItems)]
		public ListItems<ITestCase> TestCases
		{
			get
			{
				if (_testCases == null)
					_testCases = new ListItems<ITestCase> { Parent = this };

				return _testCases;
			}
		}
	}
}
