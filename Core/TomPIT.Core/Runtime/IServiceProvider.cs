using System;

namespace TomPIT.Runtime
{
	public interface IServiceProvider
	{
		Type ResolveServiceType(Type contract);
	}
}
