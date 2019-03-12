using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.QA
{
	public enum TestErrorBehavior
	{
		Stop = 1,
		Continue = 2
	}

	public interface ITestSuite : IConfiguration
	{
		ListItems<ITestScenario> Scenarios { get; }

		IServerEvent Prepare { get; }
		IServerEvent Complete { get; }
		IServerEvent Error { get; }
	}
}
