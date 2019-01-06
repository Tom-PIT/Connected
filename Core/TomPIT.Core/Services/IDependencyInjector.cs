using System;

namespace TomPIT.Services
{
	public interface IDependencyInjector
	{
		bool ResolveParameter(Type type, out object instance);
	}
}
