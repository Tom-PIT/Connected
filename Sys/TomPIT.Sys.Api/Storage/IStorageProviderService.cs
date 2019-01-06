using System;
using System.Collections.Generic;

namespace TomPIT.Api.Storage
{
	public interface IStorageProviderService
	{
		void Register(IStorageProvider provider);

		IStorageProvider Resolve(Guid resourceGroup);
		IStorageProvider Select(Guid token);
		List<IStorageProvider> Query();
	}
}
