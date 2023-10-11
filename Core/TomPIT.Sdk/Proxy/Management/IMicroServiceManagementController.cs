using System;
using System.Collections.Immutable;
using TomPIT.ComponentModel;

namespace TomPIT.Proxy.Management
{
	public interface IMicroServiceManagementController
	{
		void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStages supportedStages, string version);
		void Delete(Guid token);
		void Update(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStages supportedStages);
		ImmutableList<IMicroService> Query(Guid resourceGroup);

	}
}
