using System;
using System.Collections.Generic;
using TomPIT.Storage;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareStorageService
	{
		Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic);
		Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic, Guid token);
		Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic, string draft);

		void CommitDrafts(string draft, string primaryKey);
		List<IBlob> QueryDrafts(string draft);
		List<IBlob> Query([CAP(CAP.MicroservicesProvider)]string microService, string primaryKey);

		void Delete(Guid blob);
		void Delete([CAP(CAP.MicroservicesProvider)]string microService, string primaryKey);

		byte[] Download(Guid blob);
		byte[] Download([CAP(CAP.MicroservicesProvider)]string microService, string primaryKey);
	}
}