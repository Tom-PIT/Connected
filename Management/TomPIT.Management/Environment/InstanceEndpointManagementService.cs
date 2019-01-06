using Newtonsoft.Json.Linq;
using System;
using TomPIT.Net;

namespace TomPIT.Environment
{
	internal class InstanceEndpointManagementService : IInstanceEndpointManagementService
	{
		public InstanceEndpointManagementService(ISysContext server)
		{
			Server = server;
		}

		private ISysContext Server { get; }

		public void Delete(Guid instance)
		{
			var u = Server.CreateUrl("InstanceEndpointManagement", "Delete");
			var e = new JObject
			{
				{"token", instance }
			};

			Server.Connection.Post(u, e);

			if (Server.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
				n.NotifyRemoved(this, new InstanceEndpointEventArgs(instance));
		}

		public Guid Insert(string name, InstanceType type, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
		{
			var u = Server.CreateUrl("InstanceEndpointManagement", "Insert");
			var e = new JObject
			{
				{"name", name },
				{"type", type.ToString() },
				{"url", url },
				{"reverseProxyUrl", reverseProxyUrl },
				{"status", status.ToString() },
				{"verbs", verbs.ToString() },
			};

			var id = Server.Connection.Post<Guid>(u, e);

			if (Server.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
				n.NotifyChanged(this, new InstanceEndpointEventArgs(id));

			return id;
		}

		public void Update(Guid instance, string name, InstanceType type, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
		{
			var u = Server.CreateUrl("InstanceEndpointManagement", "Update");
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

			Server.Connection.Post(u, e);

			if (Server.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
				n.NotifyChanged(this, new InstanceEndpointEventArgs(instance));
		}
	}
}
