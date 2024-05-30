using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Storage;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class StorageController : IStorageController
	{
		public void Commit(string draft, string primaryKey)
		{
			DataModel.Blobs.Commit(draft, primaryKey);
		}

		public void Clean(Guid microService)
		{
			DataModel.Blobs.Clean(microService);
		}

		public void Delete(Guid blob)
		{
			DataModel.Blobs.Delete(blob);
		}

		public IBlobContent Download(Guid blob)
		{
			return DataModel.BlobsContents.Select(blob);
		}

		public ImmutableList<IBlobContent> Download(List<Guid> blobs)
		{
			return DataModel.BlobsContents.Query(blobs).ToImmutableList();
		}

		public ImmutableList<IBlobContent> Download(List<Guid> resourceGroups, List<int> types)
		{
			return DataModel.BlobsContents.Query(resourceGroups, types).ToImmutableList();
		}

		public ImmutableList<IBlob> Query(Guid microService, int kind, Guid resourceGroup, string primaryKey)
		{
			return DataModel.Blobs.Query(resourceGroup, kind, primaryKey, microService, null).ToImmutableList();
		}

		public ImmutableList<IBlob> Query(Guid microService, int kind, Guid resourceGroup, string primaryKey, string topic)
		{
			return DataModel.Blobs.Query(resourceGroup, kind, primaryKey, microService, topic).ToImmutableList();
		}

		public ImmutableList<IBlob> Query(Guid microService)
		{
			return DataModel.Blobs.Query(microService).ToImmutableList();
		}

		public ImmutableList<IBlob> Query(int kind, Guid microService)
		{
			return DataModel.Blobs.Query(microService, kind).ToImmutableList();
		}

		public ImmutableList<IBlob> QueryDrafts(string draft)
		{
			return DataModel.Blobs.QueryDrafts(draft).ToImmutableList();
		}

		public void Restore(IBlob blob, byte[] content)
		{
			DataModel.Blobs.Deploy(blob.ResourceGroup, blob.Type, blob.PrimaryKey, blob.MicroService, blob.Topic, blob.FileName, blob.ContentType, content, StoragePolicy.Singleton, blob.Token, blob.Version);
		}

		public IBlob Select(Guid blob)
		{
			return DataModel.Blobs.Select(blob);
		}

		public Guid Upload(IBlob blob, byte[] content, StoragePolicy policy, Guid token)
		{
			return DataModel.Blobs.Upload(blob.ResourceGroup, blob.Type, blob.PrimaryKey, blob.MicroService, blob.Topic, blob.FileName, blob.ContentType, blob.Draft ?? "", content, policy, token);
		}

		public void Refresh(Guid token)
		{
			DataModel.Blobs.RefreshBlob(token);
		}
	}
}
