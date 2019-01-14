using TomPIT.ComponentModel.Events;

namespace TomPIT.Workers
{
	public interface IHostedWorker : IWorker
	{
		IServerEvent Invoke { get; }
	}
}
