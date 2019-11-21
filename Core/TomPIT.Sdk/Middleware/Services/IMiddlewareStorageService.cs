using System;
using System.Collections.Generic;
using TomPIT.Storage;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareStorageService
	{
		Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic);
		Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic, Guid token);
		Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic, string draft);

		void CommitDrafts(string draft, string primaryKey);
		List<IBlob> QueryDrafts(string draft);
		List<IBlob> Query(string primaryKey, string topic);
		List<IBlob> Query(string primaryKey);

		void Delete(Guid blob);
		void Delete(string primaryKey, string topic);
		void Delete(string primaryKey);

		byte[] Download(Guid blob);
		byte[] Download(string primaryKey, string topic);
		byte[] Download(string primaryKey);
	}
}