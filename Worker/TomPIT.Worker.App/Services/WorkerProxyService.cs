using System;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Worker.Services
{
	internal class WorkerProxyService : TenantObject, IWorkerProxyService
	{
		public WorkerProxyService(ITenant tenant) : base(tenant)
		{
		}

		public void Ping(Guid microService, Guid popReceipt)
		{
			var url = Tenant.CreateUrl("WorkerManagement", "Ping");
			var d = new JObject
			{
				{"microService", microService },
				{"popReceipt", popReceipt }
			};

			MiddlewareDescriptor.Current.Tenant.Post(url, d);
		}

		public void Error(Guid microService, Guid popReceipt)
		{
			var url = Tenant.CreateUrl("WorkerManagement", "Error");
			var d = new JObject
			{
				{"microService", microService },
				{"popReceipt", popReceipt },
			};

			MiddlewareDescriptor.Current.Tenant.Post(url, d);
		}

		public void Complete(Guid microService, Guid popReceipt, Guid worker)
		{
			var url = Tenant.CreateUrl("WorkerManagement", "Complete");
			var d = new JObject
			{
				{"microService", microService },
				{"popReceipt", popReceipt },
				{"worker", worker }
			};

			MiddlewareDescriptor.Current.Tenant.Post(url, d);
		}

		public void AttachState(Guid worker, Guid state)
		{
			var url = Tenant.CreateUrl("WorkerManagement", "AttachState");
			var d = new JObject
			{
				{"worker", worker },
				{"state", state }
			};

			MiddlewareDescriptor.Current.Tenant.Post(url, d);
		}
	}
}
