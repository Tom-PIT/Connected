using System;
using System.Collections.Generic;
using TomPIT.Storage;

namespace TomPIT.Services.Context
{
	public interface IContextStorageService
	{
		Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic);
		Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic, Guid draft);

		void CommitDrafts(Guid draft, string primaryKey);
		List<IBlob> QueryDrafts(Guid draft);

		byte[] Download(Guid blob);
	}
}
