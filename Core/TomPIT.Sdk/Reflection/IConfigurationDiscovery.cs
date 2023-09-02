using System;
using System.Collections.Immutable;
using TomPIT.ComponentModel;

namespace TomPIT.Reflection
{
	public interface IConfigurationDiscovery
	{
		IElement Find(Guid component, Guid id);
		IText FindText(Guid component, Guid textBlob);
		IText Find(string path);
		IElement Find(IConfiguration configuration, Guid id);
		ImmutableList<T> Query<T>(IConfiguration configuration) where T : IElement;
		ImmutableList<Guid> QueryDependencies(IConfiguration configuration);
	}
}
