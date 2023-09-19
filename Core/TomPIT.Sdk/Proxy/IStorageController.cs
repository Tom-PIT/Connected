using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Storage;

namespace TomPIT.Proxy
{
	public interface IStorageController
	{
		void Commit(string draft, string primaryKey);
		void Delete(Guid blob);
		IBlob Select(Guid blob);
		void Clean(Guid microService);
		ImmutableList<IBlob> Query(Guid microService, int kind, Guid resourceGroup, string primaryKey);
		ImmutableList<IBlob> Query(Guid microService, int kind, Guid resourceGroup, string primaryKey, string topic);
		ImmutableList<IBlob> QueryDrafts(string draft);
		ImmutableList<IBlob> Query(Guid microService);
		ImmutableList<IBlob> Query(int kind, Guid microService);
		void Restore(IBlob blob, byte[] content);
		Guid Upload(IBlob blob, byte[] content, StoragePolicy policy, Guid token);
		IBlobContent Download(Guid blob);
		ImmutableList<IBlobContent> Download(List<Guid> blobs);
		ImmutableList<IBlobContent> Download(List<Guid> resourceGroups, List<int> types);
	}
}
