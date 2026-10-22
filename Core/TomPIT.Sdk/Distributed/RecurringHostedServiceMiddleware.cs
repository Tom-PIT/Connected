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
		/*
		 * The instance has its own source so its loop can be stopped when its configuration changes,
		 * otherwise every change leaves the previous loop running alongside the new one. It exists from
		 * construction, so a Stop that arrives before Start still takes effect.
		 */
		private readonly CancellationTokenSource _cancel = new();
		private int _stopped;

		protected TimeSpan InvokeInterval { get; set; } = TimeSpan.FromMinutes(1);

		public async Task Start(CancellationToken cancellationToken)
		{
			if (Volatile.Read(ref _stopped) == 1)
				return;

			var lifetime = Tenant.GetService<IRuntimeService>().Host.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
			/*
			 * Both registrations are on long-lived tokens, so they must be disposed once the loop ends,
			 * otherwise those tokens keep this instance alive.
			 */
			using var outer = cancellationToken.Register(CancelSafely);
			using var stopping = lifetime.ApplicationStopping.Register(async () =>
			{
				await Stop(CancellationToken.None);
			});

			var token = _cancel.Token;

			if (token.IsCancellationRequested)
				return;

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

		protected async virtual Task OnStart(CancellationToken cancellationToken)
		{
			await Task.CompletedTask;
		}

		public async Task Stop(CancellationToken cancellationToken)
		{
			if (Interlocked.Exchange(ref _stopped, 1) == 1)
				return;

			CancelSafely();
			/*
			 * Stop is awaited from async void callers (configuration events, application stopping),
			 * so an exception from the service's own OnStop would terminate the process.
			 */
			try
			{
				await OnStop(cancellationToken);
			}
			catch (Exception ex)
			{
				Tenant.LogError(GetType().Name, ex.Message);
			}
		}

		private void CancelSafely()
		{
			try
			{
				_cancel.Cancel();
			}
			catch (Exception ex)
			{
				Tenant.LogError(GetType().Name, ex.Message);
			}
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
