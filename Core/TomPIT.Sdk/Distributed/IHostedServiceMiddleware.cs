using System.Threading;
using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.Distributed
{
	public interface IHostedServiceMiddleware : IMiddlewareComponent
	{
		Task Start(CancellationToken cancellationToken);
		Task Stop(CancellationToken cancellationToken);
		//protected abstract Task Execute(CancellationToken stoppingToken);
	}
}
