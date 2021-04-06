using System;
using System.Collections.Immutable;
using TomPIT.ComponentModel;

namespace TomPIT.Reflection
{
	public interface IMicroServiceReferencesDiscovery
	{
		IServiceReferencesConfiguration Select(string microService);
		IServiceReferencesConfiguration Select(Guid microService);
		[Obsolete("Please use References instead.")]
		ImmutableArray<IMicroService> Flatten(Guid microService);
		ImmutableArray<IMicroService> ReferencedBy(Guid microService, bool recursive);
		ImmutableArray<IMicroService> References(Guid microService, bool recursive);
	}
}
