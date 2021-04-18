using System;
using System.Collections.Generic;
using TomPIT.Environment;
using TomPIT.Storage;

namespace TomPIT.SysDb.Storage
{
	/*
	 * Microservice must passed as a guid because it is not necessarry that a microservice actually exist at the moment
	 * of uploading procecure which usually uses this method for querying existing blobs
	 */
	public interface IStorageHandler
	{
		List<IBlob> QueryOrphaned(DateTime modified);
		void Commit(string draft, string primaryKey);
		void Delete(IBlob blob);
		IBlob Select(Guid token);
		List<IBlob> Query(List<Guid> blobs);
		List<IBlob> QueryByLevel(Guid microService, int level);
		List<IBlob> Query(IResourceGroup resourceGroup, int type, string primaryKey);
		List<IBlob> Query(IResourceGroup resourceGroup, int type, string primaryKey, Guid microService, string topic);
		List<IBlob> Query(Guid microService);
		List<IBlob> Query(Guid microService, int type);
		List<IBlob> QueryDrafts(string draft);
		void Insert(IResourceGroup resourceGroup, Guid token, int type, string primaryKey, Guid microService, string topic, string fileName, string contentType, int size, int version, DateTime modified, string draft);
		void Update(IBlob blob, string primaryKey, string fileName, string contentType, int size, int version, DateTime modified, string draft);
	}
}
