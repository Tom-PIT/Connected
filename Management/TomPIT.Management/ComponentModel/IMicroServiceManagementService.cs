using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Deployment;

namespace TomPIT.Management.ComponentModel
{
	public interface IMicroServiceManagementService
	{
		void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, IPackage package, string version);
		void Update(Guid microService, string name, MicroServiceStatus status, Guid template, Guid resourceGroup, Guid package, Guid plan, UpdateStatus updateStatus, CommitStatus commitStatus);
		void Delete(Guid microService);

		List<IMicroService> Query(Guid resourceGroup);
		List<IMicroServiceString> QueryStrings(Guid microService);
	}
}
