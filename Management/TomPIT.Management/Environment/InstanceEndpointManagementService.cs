using System;
using Newtonsoft.Json.Linq;
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
			var u = Tenant.CreateUrl("InstanceEndpointManagement", "Delete");
			var e = new JObject
			{
				{"token", instance }
			};

			Tenant.Post(u, e);

			if (Tenant.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
				n.NotifyRemoved(this, new InstanceEndpointEventArgs(instance));
		}

		public Guid Insert(string name, InstanceType type, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
		{
			var u = Tenant.CreateUrl("InstanceEndpointManagement", "Insert");
			var e = new JObject
			{
				{"name", name },
				{"type", type.ToString() },
				{"url", url },
				{"reverseProxyUrl", reverseProxyUrl },
				{"status", status.ToString() },
				{"verbs", verbs.ToString() },
			};

			var id = Tenant.Post<Guid>(u, e);

			if (Tenant.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
				n.NotifyChanged(this, new InstanceEndpointEventArgs(id));

			return id;
		}

		public void Update(Guid instance, string name, InstanceType type, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
		{
			var u = Tenant.CreateUrl("InstanceEndpointManagement", "Update");
			var e = new JObject
			{
				{"token", instance },
				{ "name", name },
				{"type", type.ToString() },
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
