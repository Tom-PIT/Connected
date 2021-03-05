using System;
using System.Collections.Generic;
using TomPIT.ComponentModel.IoC;

namespace TomPIT.IoC
{
	public interface IUIDependencyInjectionService
	{
		List<IUIDependencyDescriptor> QueryViewDependencies(string view, object arguments);
		List<IUIDependencyDescriptor> QueryPartialDependencies(string partial, object arguments);
		List<IUIDependencyDescriptor> QueryMasterDependencies(Guid microService, string master, object arguments, MasterDependencyKind kind);
	}
}
