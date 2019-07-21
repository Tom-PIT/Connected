using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Search.Indexing;

namespace TomPIT.Search
{
	internal class IndexingService : IIndexingService
	{
		public void Complete(Guid popReceipt)
		{
			var u = Instance.Connection.CreateUrl("SearchManagement", "Complete");
			var e = new JObject
			{
				{"popReceipt", popReceipt }
			};

			Instance.Connection.Post(u, e);
		}

		public void CompleteRebuilding(Guid catalog)
		{
			var u = Instance.Connection.CreateUrl("SearchManagement", "DeleteState");
			var e = new JObject
			{
				{"catalog", catalog }
			};

			Instance.Connection.Post(u, e);
		}

		public void Flush()
		{
			IndexCache.Flush();
		}

		public void MarkRebuilding(Guid catalog)
		{
			var u = Instance.Connection.CreateUrl("SearchManagement", "UpdateState");
			var e = new JObject
			{
				{"catalog", catalog },
				{"status", CatalogStateStatus.Rebuilding.ToString() }
			};

			Instance.Connection.Post(u, e);
		}

		public void Ping(Guid popReceipt, int nextVisible)
		{
			var u = Instance.Connection.CreateUrl("SearchManagement", "Ping");
			var e = new JObject
			{
				{"popReceipt", popReceipt },
				{"nextVisible", nextVisible }
			};

			Instance.Connection.Post(u, e);
		}

		public void Rebuild(Guid catalog)
		{
			var u = Instance.Connection.CreateUrl("SearchManagement", "InvalidateState");
			var e = new JObject
			{
				{"catalog", catalog }
			};

			Instance.Connection.Post(u, e);
		}

		public void ResetRebuilding(Guid catalog)
		{
			var u = Instance.Connection.CreateUrl("SearchManagement", "UpdateState");
			var e = new JObject
			{
				{"catalog", catalog },
				{"status", CatalogStateStatus.Pending.ToString() }
			};

			Instance.Connection.Post(u, e);
		}

		public void Scave()
		{
			IndexCache.Scave();
		}

		public ICatalogState SelectState(Guid catalog)
		{
			var u = Instance.Connection.CreateUrl("SearchManagement", "SelectState");
			var e = new JObject
			{
				{"catalog", catalog }
			};

			return Instance.Connection.Post<CatalogState>(u, e);
		}
	}
}
