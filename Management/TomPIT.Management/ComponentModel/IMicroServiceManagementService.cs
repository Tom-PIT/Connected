using System;
using System.Collections.Generic;
using TomPIT.Deployment;

namespace TomPIT.ComponentModel
{
	public interface IMicroServiceManagementService
	{
		void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, IPackage package);
		void Update(Guid microService, string name, MicroServiceStatus status, Guid template, Guid resourceGroup, Guid package);
		void Delete(Guid microService);

		List<IMicroService> Query(Guid resourceGroup);
		List<IMicroServiceString> QueryStrings(Guid microService);
	}
}
