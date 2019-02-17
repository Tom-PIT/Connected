using LZ4;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Storage
{
	internal class StorageService : ClientRepository<IBlob, Guid>, IStorageService, IStorageNotification
	{
		private BlobContentCache _blobContent = null;

		public event BlobChangedHandler BlobChanged;
		public event BlobChangedHandler BlobRemoved;
		public event BlobChangedHandler BlobAdded;
		public event BlobChangedHandler BlobCommitted;

		public StorageService(ISysConnection connection) : base(connection, "blob")
		{
			_blobContent = new BlobContentCache(Connection);
			Connection.GetService<IMicroServiceService>().MicroServiceInstalled += OnMicroServiceInstalled;
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

		public void Commit(Guid draft, string primaryKey)
		{
			var u = Connection.CreateUrl("Storage", "Commit");
			var args = new JObject
			{
				{ "draft", draft },
				{ "primaryKey", primaryKey }
			};

			Connection.Post(u, args);
			Remove(f => f.Draft == draft && string.Compare(f.PrimaryKey, primaryKey, true) == 0);
		}

		public void Delete(Guid blob)
		{
			var u = Connection.CreateUrl("Storage", "Delete");
			var args = new JObject
			{
				{ "blob", blob }
			};

			Connection.Post(u, args);
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
			var u = Connection.CreateUrl("Storage", "Query")
				.AddParameter("microService", microService)
				.AddParameter("type", type)
				.AddParameter("resourceGroup", resourceGroup)
				.AddParameter("primaryKey", primaryKey);

			return Connection.Get<List<Blob>>(u).ToList<IBlob>();
		}

		public List<IBlob> Query(Guid microService)
		{
			var u = Connection.CreateUrl("Storage", "QueryByMicroService")
				.AddParameter("microService", microService);

			return Connection.Get<List<Blob>>(u).ToList<IBlob>();
		}

		public List<IBlob> QueryDrafts(Guid draft)
		{
			var u = Connection.CreateUrl("Storage", "QueryDrafts")
				.AddParameter("draft", draft);

			return Connection.Get<List<Blob>>(u).ToList<IBlob>();
		}

		public IBlob Select(Guid blob)
		{
			return Get(blob,
				(f) =>
				{
					var u = Connection.CreateUrl("Storage", "Select")
						.AddParameter("blob", blob);

					return Connection.Get<Blob>(u);
				});
		}

		public Guid Upload(IBlob blob, byte[] content, StoragePolicy policy)
		{
			return Upload(blob, content, policy, Guid.Empty);
		}

		public Guid Upload(IBlob blob, byte[] content, StoragePolicy policy, Guid token)
		{
			var compressed = content == null ? null : LZ4Codec.Wrap(content);

			var u = Connection.CreateUrl("Storage", "Upload");
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

			var r = Connection.Post<Guid>(u, args);

			BlobContent.Delete(r);

			return r;

		}

		public void Restore(IBlob blob, byte[] content)
		{
			var compressed = content == null ? null : LZ4Codec.Wrap(content);

			var u = Connection.CreateUrl("Storage", "Deploy");
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

			var r = Connection.Post<Guid>(u, args);

			BlobContent.Delete(r);
		}

		public void NotifyChanged(object sender, BlobEventArgs e)
		{
			Remove(e.Blob);
			BlobContent.Delete(e.Blob);

			BlobChanged?.Invoke(Connection, e);
		}

		public void NotifyRemoved(object sender, BlobEventArgs e)
		{
			Remove(e.Blob);
			BlobContent.Delete(e.Blob);

			BlobRemoved?.Invoke(Connection, e);
		}

		public void NotifyAdded(object sender, BlobEventArgs e)
		{
			BlobAdded?.Invoke(Connection, e);
		}

		public void NotifyCommitted(object sender, BlobEventArgs e)
		{
			Remove(e.Blob);
			BlobContent.Delete(e.Blob);

			BlobCommitted?.Invoke(Connection, e);
		}

		private BlobContentCache BlobContent { get { return _blobContent; } }
	}
}
