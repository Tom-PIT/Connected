using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.Workers
{
	public interface IHostedWorker : IWorker
	{
		IServerEvent Invoke { get; }
	}
}
