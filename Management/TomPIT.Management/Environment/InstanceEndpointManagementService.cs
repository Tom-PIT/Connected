using Newtonsoft.Json.Linq;
using System;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Middleware;

namespace TomPIT.Management.Environment
{
    internal class InstanceEndpointManagementService : TenantObject, IInstanceEndpointManagementService
    {
        public InstanceEndpointManagementService(ITenant tenant) : base(tenant)
        {

        }

        public void Delete(Guid instance)
        {
            Instance.SysProxy.Management.InstanceEndpoints.Delete(instance);

            if (Tenant.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
                n.NotifyRemoved(this, new InstanceEndpointEventArgs(instance));
        }

        public Guid Insert(string name, InstanceFeatures features, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
        {
            var id = Instance.SysProxy.Management.InstanceEndpoints.Insert(features, name, url, reverseProxyUrl, status, verbs);

            if (Tenant.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
                n.NotifyChanged(this, new InstanceEndpointEventArgs(id));

            return id;
        }

        public void Update(Guid instance, string name, InstanceFeatures features, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
        {
            var u = Tenant.CreateUrl("InstanceEndpointManagement", "Update");
            var e = new JObject
            {
                {"token", instance },
                { "name", name },
                {"type", features.ToString() },
                {"url", url },
                {"reverseProxyUrl", reverseProxyUrl },
                {"status", status.ToString() },
                {"verbs", verbs.ToString() },
            };

            Tenant.Post(u, e);

            if (Tenant.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
                n.NotifyChanged(this, new InstanceEndpointEventArgs(instance));
        }
    }
}
