using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.Management.ComponentModel
{
	public interface IMicroServiceManagementService
	{
		void Insert(Guid token, string name, Guid resourceGroup, Guid template, string version, string commit);
		void Update(Guid microService, string name, Guid template, Guid resourceGroup, string version, string commit);
		void Delete(Guid microService);

		List<IMicroService> Query(Guid resourceGroup);
	}
}
