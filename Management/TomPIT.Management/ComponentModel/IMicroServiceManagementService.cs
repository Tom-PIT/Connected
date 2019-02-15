using System;
using System.Collections.Generic;

namespace TomPIT.ComponentModel
{
	public interface IMicroServiceManagementService
	{
		Guid Insert(string name, Guid resourceGroup, Guid template, MicroServiceStatus status);
		void Update(Guid microService, string name, MicroServiceStatus status, Guid template, Guid resourceGroup, Guid package, Guid configuration);
		void Delete(Guid microService);

		List<IMicroService> Query(Guid resourceGroup);
		List<IMicroServiceString> QueryStrings(Guid microService);
	}
}
