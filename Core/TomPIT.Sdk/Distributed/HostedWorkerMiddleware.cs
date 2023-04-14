using System;
using System.Linq;
using System.Runtime.ExceptionServices;

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

            if (Shell.GetConfiguration<IClientSys>().HealthMonitoredMiddleware.FirstOrDefault(e => string.Compare(e.Type, this.GetType().Name) == 0) is MiddlewareHealthMonitoringConfiguration config)
               SendHealthMonitoringHeartbeat(config);

         }
         catch (Exception ex)
         {
            Rollback();

            var se = new ScriptException(this, TomPITException.Unwrap(this, ex));

            ExceptionDispatchInfo.Capture(se).Throw();
         }
      }

      protected abstract void OnInvoke();

      private void SendHealthMonitoringHeartbeat(MiddlewareHealthMonitoringConfiguration config)
      {
         if (string.IsNullOrWhiteSpace(config?.EndpointKey) || string.IsNullOrWhiteSpace(config?.SubscriptionKey) || string.IsNullOrWhiteSpace(config?.RestToken) || string.IsNullOrWhiteSpace(config?.EndpointUrl))
            return;

         try
         {
            var authProvider = new BearerAuthenticationProvider(config.RestToken);
            var client = MiddlewareDescriptor.Current.Tenant.GetService<IHealthMonitoringClientFactory>().Select(config.EndpointUrl, config.SubscriptionKey, authProvider);

            if (client is null)
               return;

            AsyncUtils.RunSync(() => client.Requests.Measurements.Insert(new Connected.SaaS.Clients.HealthMonitoring.Endpoint { Key = config.EndpointKey }, 100, default));
         }
         catch
         {
         }
      }
   }
}
