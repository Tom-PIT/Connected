using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.QA;

namespace TomPIT.Application.QA
{
	[Create("Scenario", nameof(Name))]
	public class TestScenario : TestElement, ITestScenario
	{
		private ListItems<ITestCase> _testCases = null;

		[Items("TomPIT.Application.Design.Items.TestCasesCollection, TomPIT.Application.Design")]
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
