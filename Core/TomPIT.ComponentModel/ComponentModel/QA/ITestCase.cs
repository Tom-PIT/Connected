using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.QA
{

	public interface ITestCase : ITestElement
	{
		IServerEvent Invoke { get; }
	}
}
