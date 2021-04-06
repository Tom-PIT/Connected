using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Connectivity;

namespace TomPIT.Storage
{
	public delegate void BlobChangedHandler(ITenant sender, BlobEventArgs e);

	public interface IStorageService
	{
		event BlobChangedHandler BlobChanged;
		event BlobChangedHandler BlobRemoved;
		event BlobChangedHandler BlobAdded;
		event BlobChangedHandler BlobCommitted;

		void Commit(string draft, string primaryKey);
		void Delete(Guid blob);
		IBlob Select(Guid blob);
		List<IBlob> Query(Guid microService, int kind, Guid resourceGroup, string primaryKey);
		List<IBlob> Query(Guid microService, int kind, Guid resourceGroup, string primaryKey, string topic);
		List<IBlob> QueryDrafts(string draft);
		List<IBlob> Query(Guid microService);
		void Restore(IBlob blob, byte[] content);
		Guid Upload(IBlob blob, byte[] content, StoragePolicy policy);
		Guid Upload(IBlob blob, byte[] content, StoragePolicy policy, Guid token);
		IBlobContent Download(Guid blob);
		IBlobContent Download(Guid microService, int kind, Guid resourceGroup, string primaryKey);
		IBlobContent Download(Guid microService, int kind, Guid resourceGroup, string primaryKey, string topic);
		List<IBlobContent> Download(List<Guid> blobs);

		ImmutableList<Guid> Preload(int kind, Guid microService);
		ImmutableList<Guid> Preload(int kind);
		void Release(Guid blob);
	}
}
