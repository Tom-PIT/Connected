using System;

namespace TomPIT.Runtime
{
	public interface IDependencyInjector
	{
		bool ResolveParameter(Type type, out object instance);
	}
}
