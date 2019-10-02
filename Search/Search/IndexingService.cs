using System;
using Newtonsoft.Json.Linq;
using TomPIT.Middleware;
using TomPIT.Search.Indexing;

namespace TomPIT.Search
{
	internal class IndexingService : IIndexingService
	{
		public void Complete(Guid popReceipt)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("SearchManagement", "Complete");
			var e = new JObject
			{
				{"popReceipt", popReceipt }
			};

			MiddlewareDescriptor.Current.Tenant.Post(u, e);
		}

		public void CompleteRebuilding(Guid catalog)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("SearchManagement", "DeleteState");
			var e = new JObject
			{
				{"catalog", catalog }
			};

			MiddlewareDescriptor.Current.Tenant.Post(u, e);
		}

		public void Flush()
		{
			IndexCache.Flush();
		}

		public void MarkRebuilding(Guid catalog)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("SearchManagement", "UpdateState");
			var e = new JObject
			{
				{"catalog", catalog },
				{"status", CatalogStateStatus.Rebuilding.ToString() }
			};

			MiddlewareDescriptor.Current.Tenant.Post(u, e);
		}

		public void Ping(Guid popReceipt, int nextVisible)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("SearchManagement", "Ping");
			var e = new JObject
			{
				{"popReceipt", popReceipt },
				{"nextVisible", nextVisible }
			};

			MiddlewareDescriptor.Current.Tenant.Post(u, e);
		}

		public void Rebuild(Guid catalog)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("SearchManagement", "InvalidateState");
			var e = new JObject
			{
				{"catalog", catalog }
			};

			MiddlewareDescriptor.Current.Tenant.Post(u, e);
		}

		public void ResetRebuilding(Guid catalog)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("SearchManagement", "UpdateState");
			var e = new JObject
			{
				{"catalog", catalog },
				{"status", CatalogStateStatus.Pending.ToString() }
			};

			MiddlewareDescriptor.Current.Tenant.Post(u, e);
		}

		public void Scave()
		{
			IndexCache.Scave();
		}

		public ICatalogState SelectState(Guid catalog)
		{
			var u = MiddlewareDescriptor.Current.Tenant.CreateUrl("SearchManagement", "SelectState");
			var e = new JObject
			{
				{"catalog", catalog }
			};

			return MiddlewareDescriptor.Current.Tenant.Post<CatalogState>(u, e);
		}
	}
}
