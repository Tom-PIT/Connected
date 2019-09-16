using System;
using System.Collections.Generic;
using System.Linq;
using LZ4;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Storage
{
	internal class StorageService : ClientRepository<IBlob, Guid>, IStorageService, IStorageNotification
	{
		private BlobContentCache _blobContent = null;

		public event BlobChangedHandler BlobChanged;
		public event BlobChangedHandler BlobRemoved;
		public event BlobChangedHandler BlobAdded;
		public event BlobChangedHandler BlobCommitted;

		public StorageService(ITenant tenant) : base(tenant, "blob")
		{
			_blobContent = new BlobContentCache(Tenant);
			Tenant.GetService<IMicroServiceService>().MicroServiceInstalled += OnMicroServiceInstalled;
		}

		private void OnMicroServiceInstalled(object sender, MicroServiceEventArgs e)
		{
			var blobs = All();

			foreach (var i in blobs)
			{
				if (i.MicroService == e.MicroService)
				{
					Remove(i.Token);
					BlobContent.Delete(i.Token);
				}
			}
		}

		public void Commit(string draft, string primaryKey)
		{
			var u = Tenant.CreateUrl("Storage", "Commit");
			var args = new JObject
			{
				{ "draft", draft },
				{ "primaryKey", primaryKey }
			};

			Tenant.Post(u, args);
			Remove(f => string.Compare(f.Draft, draft, true) == 0 && string.Compare(f.PrimaryKey, primaryKey, true) == 0);
		}

		public void Delete(Guid blob)
		{
			var u = Tenant.CreateUrl("Storage", "Delete");
			var args = new JObject
			{
				{ "blob", blob }
			};

			Tenant.Post(u, args);
			Remove(blob);
		}

		public IBlobContent Download(Guid blob)
		{
			var b = Select(blob);

			if (b == null)
				return null;

			return BlobContent.Select(b);
		}

		public IBlobContent Download(Guid microService, int type, Guid resourceGroup, string primaryKey)
		{
			var r = Get(f => f.MicroService == microService && f.Type == type && string.Compare(f.PrimaryKey, primaryKey, true) == 0);

			if (r == null)
			{
				var rs = Query(microService, type, resourceGroup, primaryKey);

				if (rs == null || rs.Count == 0)
					return null;

				r = rs[0];
			}

			return Download(r.Token);
		}

		public List<IBlobContent> Download(List<Guid> blobs)
		{
			return BlobContent.Query(blobs);
		}

		public List<IBlob> Query(Guid microService, int type, Guid resourceGroup, string primaryKey)
		{
			var u = Tenant.CreateUrl("Storage", "Query")
				.AddParameter("microService", microService)
				.AddParameter("type", type)
				.AddParameter("resourceGroup", resourceGroup)
				.AddParameter("primaryKey", primaryKey);

			return Tenant.Get<List<Blob>>(u).ToList<IBlob>();
		}

		public List<IBlob> Query(Guid microService)
		{
			var u = Tenant.CreateUrl("Storage", "QueryByMicroService")
				.AddParameter("microService", microService);

			return Tenant.Get<List<Blob>>(u).ToList<IBlob>();
		}

		public List<IBlob> QueryDrafts(string draft)
		{
			var u = Tenant.CreateUrl("Storage", "QueryDrafts")
				.AddParameter("draft", draft);

			return Tenant.Get<List<Blob>>(u).ToList<IBlob>();
		}

		public IBlob Select(Guid blob)
		{
			return Get(blob,
				(f) =>
				{
					var u = Tenant.CreateUrl("Storage", "Select")
						.AddParameter("blob", blob);

					return Tenant.Get<Blob>(u);
				});
		}

		public Guid Upload(IBlob blob, byte[] content, StoragePolicy policy)
		{
			return Upload(blob, content, policy, Guid.Empty);
		}

		public Guid Upload(IBlob blob, byte[] content, StoragePolicy policy, Guid token)
		{
			var compressed = content == null ? null : LZ4Codec.Wrap(content);

			var u = Tenant.CreateUrl("Storage", "Upload");
			var args = new JObject
			{
				{"resourceGroup", blob.ResourceGroup },
				{"type", blob.Type },
				{"primaryKey", blob.PrimaryKey },
				{"microService", blob.MicroService },
				{"topic", blob.Topic },
				{"fileName", blob.FileName },
				{"contentType", blob.ContentType },
				{"draft", blob.Draft },
				{"content", compressed },
				{"policy", policy.ToString() },
				{"token", token.ToString() }
			};

			var r = Tenant.Post<Guid>(u, args);

			Remove(r);
			BlobContent.Delete(r);

			return r;

		}

		public void Restore(IBlob blob, byte[] content)
		{
			var compressed = content == null ? null : LZ4Codec.Wrap(content);

			var u = Tenant.CreateUrl("Storage", "Deploy");
			var args = new JObject
			{
				{"resourceGroup", blob.ResourceGroup },
				{"type", blob.Type },
				{"primaryKey", blob.PrimaryKey },
				{"microService", blob.MicroService },
				{"topic", blob.Topic },
				{"fileName", blob.FileName },
				{"contentType", blob.ContentType },
				{"draft", blob.Draft },
				{"content", compressed },
				{"policy", StoragePolicy.Singleton.ToString() },
				{"token", blob.Token.ToString() },
				{"version", blob.Version }
			};

			var r = Tenant.Post<Guid>(u, args);

			BlobContent.Delete(r);
		}

		public void NotifyChanged(object sender, BlobEventArgs e)
		{
			Remove(e.Blob);
			BlobContent.Delete(e.Blob);

			BlobChanged?.Invoke(Tenant, e);
		}

		public void NotifyRemoved(object sender, BlobEventArgs e)
		{
			Remove(e.Blob);
			BlobContent.Delete(e.Blob);

			BlobRemoved?.Invoke(Tenant, e);
		}

		public void NotifyAdded(object sender, BlobEventArgs e)
		{
			BlobAdded?.Invoke(Tenant, e);
		}

		public void NotifyCommitted(object sender, BlobEventArgs e)
		{
			Remove(e.Blob);
			BlobContent.Delete(e.Blob);

			BlobCommitted?.Invoke(Tenant, e);
		}

		private BlobContentCache BlobContent { get { return _blobContent; } }
	}
}
