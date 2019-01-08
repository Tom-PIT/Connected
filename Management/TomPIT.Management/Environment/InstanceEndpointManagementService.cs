using Newtonsoft.Json.Linq;
using System;
using TomPIT.Connectivity;

namespace TomPIT.Environment
{
	internal class InstanceEndpointManagementService : IInstanceEndpointManagementService
	{
		public InstanceEndpointManagementService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void Delete(Guid instance)
		{
			var u = Connection.CreateUrl("InstanceEndpointManagement", "Delete");
			var e = new JObject
			{
				{"token", instance }
			};

			Connection.Post(u, e);

			if (Connection.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
				n.NotifyRemoved(this, new InstanceEndpointEventArgs(instance));
		}

		public Guid Insert(string name, InstanceType type, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
		{
			var u = Connection.CreateUrl("InstanceEndpointManagement", "Insert");
			var e = new JObject
			{
				{"name", name },
				{"type", type.ToString() },
				{"url", url },
				{"reverseProxyUrl", reverseProxyUrl },
				{"status", status.ToString() },
				{"verbs", verbs.ToString() },
			};

			var id = Connection.Post<Guid>(u, e);

			if (Connection.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
				n.NotifyChanged(this, new InstanceEndpointEventArgs(id));

			return id;
		}

		public void Update(Guid instance, string name, InstanceType type, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
		{
			var u = Connection.CreateUrl("InstanceEndpointManagement", "Update");
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

			Connection.Post(u, e);

			if (Connection.GetService<IInstanceEndpointService>() is IInstanceEndpointNotification n)
				n.NotifyChanged(this, new InstanceEndpointEventArgs(instance));
		}
	}
}
