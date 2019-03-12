namespace TomPIT.ComponentModel.QA
{
	public interface ITestScenario : ITestElement
	{
		ListItems<ITestCase> TestCases { get; }
	}
}
