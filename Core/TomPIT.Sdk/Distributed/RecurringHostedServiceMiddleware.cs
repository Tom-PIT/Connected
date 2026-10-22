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
		private CancellationTokenSource _cancel;
		private CancellationTokenRegistration _stopping;
		private int _stopped;

		protected TimeSpan InvokeInterval { get; set; } = TimeSpan.FromMinutes(1);

		public async Task Start(CancellationToken cancellationToken)
		{
			var lifetime = Tenant.GetService<IRuntimeService>().Host.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
			/*
			 * The instance gets its own token so its loop can be stopped when its configuration changes,
			 * otherwise every change leaves the previous loop running alongside the new one.
			 */
			_cancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			_stopping = lifetime.ApplicationStopping.Register(async () =>
			{
				await Stop(CancellationToken.None);
			});

			var token = _cancel.Token;

			try
			{
				await OnStart(token);

				do
				{
					try
					{
						await OnInvoke(token);
					}
					catch (Exception ex)
					{
						Tenant.LogError(GetType().Name, ex.Message);
					}

					if (InvokeInterval == TimeSpan.Zero)
						break;

					await Task.Delay(InvokeInterval, token);
				}
				while (!token.IsCancellationRequested);
			}
			finally
			{
				/*
				 * The linked source is registered on the container's token, so it must be disposed
				 * once the loop ends, otherwise the container keeps this instance alive.
				 */
				_cancel.Dispose();
			}
		}

		protected async virtual Task OnStart(CancellationToken cancellationToken)
		{
			await Task.CompletedTask;
		}

		public async Task Stop(CancellationToken cancellationToken)
		{
			if (Interlocked.Exchange(ref _stopped, 1) == 1)
				return;

			_stopping.Dispose();

			try
			{
				_cancel?.Cancel();
			}
			catch (ObjectDisposedException)
			{
			}

			await OnStop(cancellationToken);
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
