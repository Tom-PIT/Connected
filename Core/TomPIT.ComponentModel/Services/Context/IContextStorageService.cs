using System;
using System.Collections.Generic;
using TomPIT.Annotations;
using TomPIT.Storage;

namespace TomPIT.Services.Context
{
	public interface IContextStorageService
	{
		Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic);
		Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic, Guid token);
		Guid Upload(StoragePolicy policy, string fileName, string contentType, string primaryKey, byte[] content, string topic, string draft);

		void CommitDrafts(string draft, string primaryKey);
		List<IBlob> QueryDrafts(string draft);
		List<IBlob> Query([CodeAnalysisProvider(CodeAnalysisProviderAttribute.MicroservicesProvider)]string microService, string primaryKey);

		void Delete(Guid blob);
		void Delete([CodeAnalysisProvider(CodeAnalysisProviderAttribute.MicroservicesProvider)]string microService, string primaryKey);

		byte[] Download(Guid blob);
		byte[] Download([CodeAnalysisProvider(CodeAnalysisProviderAttribute.MicroservicesProvider)]string microService, string primaryKey);
	}
}