using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Environment;
using TomPIT.Storage;

namespace TomPIT.SysDb.Storage
{
	public interface IStorageHandler
	{
		List<IBlob> QueryOrphaned(DateTime modified);
		void Commit(Guid draft, string primaryKey);
		void Delete(IBlob blob);
		IBlob Select(Guid token);
		List<IBlob> Query(List<Guid> blobs);
		List<IBlob> Query(IResourceGroup resourceGroup, int type, string primaryKey);
		List<IBlob> Query(IResourceGroup resourceGroup, int type, string primaryKey, IMicroService service, string topic);
		List<IBlob> Query(IMicroService microService);
		List<IBlob> QueryDrafts(Guid draft);

		void Insert(IResourceGroup resourceGroup, Guid token, int type, string primaryKey, IMicroService microService, string topic, string fileName, string contentType, int size, int version, DateTime modified, Guid draft);
		void Update(IBlob blob, string primaryKey, string fileName, string contentType, int size, int version, DateTime modified, Guid draft);
	}
}
