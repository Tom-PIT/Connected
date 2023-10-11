using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.Management.ComponentModel
{
	public interface IMicroServiceManagementService
	{
		void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStages supportedStages, string version);
		void Update(Guid microService, string name, MicroServiceStages supportedStages, Guid template, Guid resourceGroup);
		void Delete(Guid microService);

		List<IMicroService> Query(Guid resourceGroup);
	}
}
