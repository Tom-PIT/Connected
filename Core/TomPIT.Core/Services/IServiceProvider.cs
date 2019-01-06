using System;

namespace TomPIT.Services
{
	public interface IServiceProvider
	{
		Type ResolveServiceType(Type contract);
	}
}
