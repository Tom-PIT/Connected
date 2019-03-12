using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.QA
{
	public interface ITestElement : IConfigurationElement
	{
		string Name { get; }
		bool Enabled { get; }

		TestErrorBehavior ErrorBehavior { get; }

		IServerEvent Prepare { get; }
		IServerEvent Complete { get; }
		IServerEvent Error { get; }
	}
}
