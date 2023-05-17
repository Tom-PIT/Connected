using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.ServiceProviders.HealthMonitoring;

namespace TomPIT.Distributed
{
	public abstract class HostedWorkerMiddleware : LifetimeMiddleware, IHostedWorkerMiddleware
	{
		public void Invoke()
		{
			Invoke(null);
		}
		public void Invoke(IMiddlewareContext context)
		{
			if (context != null)
				this.WithContext(context);

			Validate();

			try
			{
				OnInvoke();
				Invoked();
				HealthMonitoringMeasure();
			}
			catch (Exception ex)
			{
				Rollback();

				var se = new ScriptException(this, TomPITException.Unwrap(this, ex));

				ExceptionDispatchInfo.Capture(se).Throw();
			}
		}

		protected abstract void OnInvoke();

		private void HealthMonitoringMeasure()
		{
			if (Shell.GetService<IHealthMonitoringService>() is not IHealthMonitoringService service)
				return;

			if (service.Configuration.Endpoints.FirstOrDefault(f => string.Equals(f.Name, GetType().Name, StringComparison.OrdinalIgnoreCase)) is not IEndpointConfiguration endpoint)
				return;

			try
			{
				AsyncUtils.RunSync(() => service.Measurements.Insert(endpoint.Subscription, endpoint.Name, 100));
			}
			catch
			{
			}
		}
	}
}
