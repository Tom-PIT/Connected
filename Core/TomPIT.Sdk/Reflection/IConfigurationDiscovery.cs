using System;
using System.Collections.Immutable;
using TomPIT.ComponentModel;

namespace TomPIT.Reflection
{
	public enum SearchMode
	{
		Element = 1,
		Blob = 2
	}

	public interface IConfigurationDiscovery
	{
		IElement Find(Guid component, Guid id);
		IElement Find(Guid component, Guid id, SearchMode mode);
		IText Find(string path);
		IElement Find(IConfiguration configuration, Guid id);
		ImmutableList<T> Query<T>(IConfiguration configuration) where T : IElement;
		ImmutableList<Guid> QueryDependencies(IConfiguration configuration);
	}
}
