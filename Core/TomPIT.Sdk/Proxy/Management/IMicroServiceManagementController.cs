using System;
using System.Collections.Immutable;
using TomPIT.ComponentModel;

namespace TomPIT.Proxy.Management
{
	public interface IMicroServiceManagementController
	{
		void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStages supportedStages, string version, string commit);
		void Delete(Guid token);
		void Update(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStages supportedStages, string version, string commit);
		ImmutableList<IMicroService> Query(Guid resourceGroup);

	}
}
