using System;
using System.Collections.Generic;
using System.Linq;
using LZ4;
using Newtonsoft.Json.Linq;
using TomPIT.Net;

namespace TomPIT.Storage
{
	internal class StorageService : ContextCacheRepository<IBlob, Guid>, IStorageService, IStorageNotification
	{
		private BlobContentCache _blobContent = null;

		public event BlobChangedHandler BlobChanged;
		public event BlobChangedHandler BlobRemoved;
		public event BlobChangedHandler BlobAdded;
		public event BlobChangedHandler BlobCommitted;

		public StorageService(ISysContext server) : base(server, "blob")
		{
			_blobContent = new BlobContentCache(Server);
		}

		public void Commit(Guid draft, string primaryKey)
		{
			var u = Server.CreateUrl("Storage", "Commit");
			var args = new JObject
			{
				{ "draft", draft },
				{ "primaryKey", primaryKey }
			};

			Server.Connection.Post(u, args);
			Remove(f => f.Draft == draft && string.Compare(f.PrimaryKey, primaryKey, true) == 0);
		}

		public void Delete(Guid blob)
		{
			var u = Server.CreateUrl("Storage", "Delete");
			var args = new JObject
			{
				{ "blob", blob }
			};

			Server.Connection.Post(u, args);
			Remove(blob);
		}

		public IBlobContent Download(Guid blob)
		{
			var b = Select(blob);

			if (b == null)
				return null;

			return BlobContent.Select(b);
		}

		public List<IBlobContent> Download(List<Guid> blobs)
		{
			return BlobContent.Query(blobs);
		}

		public List<IBlob> Query(Guid microService, int type, Guid resourceGroup, string primaryKey)
		{
			var u = Server.CreateUrl("Storage", "Query")
				.AddParameter("microService", microService)
				.AddParameter("type", type)
				.AddParameter("resourceGroup", resourceGroup)
				.AddParameter("primaryKey", primaryKey);

			return Server.Connection.Get<List<Blob>>(u).ToList<IBlob>();
		}

		public List<IBlob> Query(Guid microService)
		{
			var u = Server.CreateUrl("Storage", "QueryByMicroService")
				.AddParameter("microService", microService);

			return Server.Connection.Get<List<Blob>>(u).ToList<IBlob>();
		}

		public List<IBlob> QueryDrafts(Guid draft)
		{
			var u = Server.CreateUrl("Storage", "QueryDrafts")
				.AddParameter("draft", draft);

			return Server.Connection.Get<List<Blob>>(u).ToList<IBlob>();
		}

		public IBlob Select(Guid blob)
		{
			return Get(blob,
				(f) =>
				{
					var u = Server.CreateUrl("Storage", "Select")
						.AddParameter("blob", blob);

					return Server.Connection.Get<Blob>(u);
				});
		}

		public Guid Upload(IBlob blob, byte[] content, StoragePolicy policy)
		{
			return Upload(blob, content, policy, Guid.Empty);
		}

		public Guid Upload(IBlob blob, byte[] content, StoragePolicy policy, Guid token)
		{
			var compressed = content == null ? null : LZ4Codec.Wrap(content);

			var u = Server.CreateUrl("Storage", "Upload");
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

			var r = Server.Connection.Post<Guid>(u, args);

			BlobContent.Delete(r);

			return r;

		}

		public void NotifyChanged(object sender, BlobEventArgs e)
		{
			Remove(e.Blob);
			BlobContent.Delete(e.Blob);

			BlobChanged?.Invoke(Server, e);
		}

		public void NotifyRemoved(object sender, BlobEventArgs e)
		{
			Remove(e.Blob);
			BlobContent.Delete(e.Blob);

			BlobRemoved?.Invoke(Server, e);
		}

		public void NotifyAdded(object sender, BlobEventArgs e)
		{
			BlobAdded?.Invoke(Server, e);
		}

		public void NotifyCommitted(object sender, BlobEventArgs e)
		{
			Remove(e.Blob);
			BlobContent.Delete(e.Blob);

			BlobCommitted?.Invoke(Server, e);
		}

		private BlobContentCache BlobContent { get { return _blobContent; } }
	}
}
