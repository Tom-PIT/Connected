using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Collections;
using TomPIT.Storage;
using Blob = TomPIT.Storage.Blob;

namespace TomPIT.Proxy.Remote
{
	internal class StorageController : IStorageController
	{
		private const string Controller = "Storage";
		public void Commit(string draft, string primaryKey)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Commit"), new
			{
				draft,
				primaryKey
			});
		}

		public void Clean(Guid microService)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Clean"), new
			{
				microService
			});
		}

		public void Delete(Guid blob)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Delete"), new
			{
				blob
			});
		}

		public IBlobContent Download(Guid blob)
		{
			var u = Connection.CreateUrl(Controller, "Download")
			.AddParameter("blob", blob);

			return Connection.Get<BlobContent>(u);
		}

		public ImmutableList<IBlobContent> Download(List<Guid> blobs)
		{
			return Connection.Post<List<BlobContent>>(Connection.CreateUrl(Controller, "DownloadBatch"), new
			{
				items = blobs
			}).ToImmutableList<IBlobContent>();
		}

		public ImmutableList<IBlobContent> Download(List<Guid> resourceGroups, List<int> types)
		{
			return Connection.Post<List<BlobContent>>(Connection.CreateUrl(Controller, "DownloadByTypes"), new
			{
				resourceGroups,
				types
			}).ToImmutableList<IBlobContent>();
		}

		public ImmutableList<IBlob> Query(int kind, Guid microService)
		{
			return Connection.Post<List<Blob>>(Connection.CreateUrl(Controller, "QueryByType"), new
			{
				microService,
				type = kind
			}).ToImmutableList<IBlob>();
		}

		public ImmutableList<IBlob> Query(Guid microService, int kind, Guid resourceGroup, string primaryKey)
		{
			var u = Connection.CreateUrl(Controller, "Query")
				.AddParameter("microService", microService)
				.AddParameter("type", kind)
				.AddParameter("resourceGroup", resourceGroup)
				.AddParameter("primaryKey", primaryKey);

			return Connection.Get<List<Blob>>(u).ToImmutableList<IBlob>();
		}

		public ImmutableList<IBlob> Query(Guid microService, int kind, Guid resourceGroup, string primaryKey, string topic)
		{
			var u = Connection.CreateUrl(Controller, "QueryByTopic")
				.AddParameter("microService", microService)
				.AddParameter("type", kind)
				.AddParameter("resourceGroup", resourceGroup)
				.AddParameter("primaryKey", primaryKey)
				.AddParameter("topic", topic);

			return Connection.Get<List<Blob>>(u).ToImmutableList<IBlob>();
		}

		public ImmutableList<IBlob> Query(Guid microService)
		{
			var u = Connection.CreateUrl(Controller, "QueryByMicroService")
				.AddParameter("microService", microService);

			return Connection.Get<List<Blob>>(u).ToImmutableList<IBlob>();

		}

		public ImmutableList<IBlob> QueryDrafts(string draft)
		{
			var u = Connection.CreateUrl(Controller, "QueryDrafts")
				.AddParameter("draft", draft);

			return Connection.Get<List<Blob>>(u).ToImmutableList<IBlob>();
		}

		public void Restore(IBlob blob, byte[] content)
		{
			Connection.Post<Guid>(Connection.CreateUrl(Controller, "Deploy"), new
			{
				blob.ResourceGroup,
				blob.Type,
				blob.PrimaryKey,
				blob.MicroService,
				blob.Topic,
				blob.FileName,
				blob.ContentType,
				blob.Draft,
				content,
				policy = StoragePolicy.Singleton.ToString(),
				blob.Token,
				blob.Version
			});
		}

		public IBlob Select(Guid blob)
		{
			var u = Connection.CreateUrl(Controller, "Select")
				.AddParameter("blob", blob);

			return Connection.Get<Blob>(u);
		}

		public Guid Upload(IBlob blob, byte[] content, StoragePolicy policy, Guid token)
		{
			return Connection.Post<Guid>(Connection.CreateUrl(Controller, "Upload"), new
			{
				blob.ResourceGroup,
				blob.Type,
				blob.PrimaryKey,
				blob.MicroService,
				blob.Topic,
				blob.FileName,
				blob.ContentType,
				blob.Draft,
				content,
				policy = policy.ToString(),
				token
			});
		}
	}
}
