using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomPIT.Middleware;
using TomPIT.Runtime;

namespace TomPIT.Distributed
{
	public abstract class RecurringHostedServiceMiddleware : MiddlewareOperation, IHostedServiceMiddleware
	{
		protected TimeSpan InvokeInterval { get; set; } = TimeSpan.FromMinutes(1);

		public async Task Start(CancellationToken cancellationToken)
		{
			var lifetime = Tenant.GetService<IRuntimeService>().Host.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

			lifetime.ApplicationStopping.Register(async () =>
			{
				await Stop(CancellationToken.None);
			});

			await OnStart(cancellationToken);

			do
			{
				try
				{
					await OnInvoke(cancellationToken);
				}
				catch (Exception ex)
				{
					Tenant.LogError(GetType().Name, ex.Message);
				}

				if (InvokeInterval == TimeSpan.Zero)
					break;

				await Task.Delay(InvokeInterval, cancellationToken);
			}
			while (!cancellationToken.IsCancellationRequested);
		}

		protected async virtual Task OnStart(CancellationToken cancellationToken)
		{
			await Task.CompletedTask;
		}

		public async Task Stop(CancellationToken cancellationToken)
		{
			await OnStart(cancellationToken);
		}

		protected async virtual Task OnStop(CancellationToken cancellationToken)
		{
			await Task.CompletedTask;
		}

		protected async virtual Task OnInvoke(CancellationToken stoppingToken)
		{
			await Task.CompletedTask;
		}
	}
}
