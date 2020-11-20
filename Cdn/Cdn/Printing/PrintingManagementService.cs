using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Storage;

namespace TomPIT.Cdn.Printing
{
	internal class PrintingManagementService : IPrintingManagementService
	{
		private List<IPrintingProvider> _providers = null;

		public PrintingManagementService()
		{
			foreach (var plugin in Instance.Plugins)
			{
				var providers = plugin.GetPrintingProviders();

				if (providers == null)
					continue;

				Providers.AddRange(providers);
			}
		}
		public void Complete(Guid popReceipt)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("PrintingManagement", "Complete");
			var e = new JObject
			{
				{ "popReceipt", popReceipt }
			};

			MiddlewareDescriptor.Current.Tenant.Post(u, e);
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("PrintingManagement", "Dequeue");
			var e = new JObject
			{
				{ "count", count }
			};

			return MiddlewareDescriptor.Current.Tenant.Post<List<QueueMessage>>(u, e).ToList<IQueueMessage>();
		}

		public void Error(Guid popReceipt, string error)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("PrintingManagement", "Error");
			var e = new JObject
			{
				{ "popReceipt", popReceipt },
				{ "error", error }
			};

			MiddlewareDescriptor.Current.Tenant.Post(u, e);
		}

		public IPrintingProvider GetProvider(string name)
		{
			if (Providers.Count == 0)
				return null;

			if (string.IsNullOrWhiteSpace(name))
				return Providers[0];

			return Providers.FirstOrDefault(f => string.Compare(f.Name, name, true) == 0);
		}

		public void Ping(Guid popReceipt)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("PrintingManagement", "Ping");
			var e = new JObject
			{
				{ "popReceipt", popReceipt },
				{ "nextVisible", TimeSpan.FromMinutes(4) }
			};

			MiddlewareDescriptor.Current.Tenant.Post(u, e);
		}

		private List<IPrintingProvider> Providers
		{
			get
			{
				if (_providers == null)
					_providers = new List<IPrintingProvider>();

				return _providers;
			}
		}
	}
}
