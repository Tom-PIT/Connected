using System;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Reflection;

namespace TomPIT.Middleware;
internal class MiddlewareTypeDescriptor
{
	private Type _type = null;
	private bool _typeInitialized = false;
	private object _sync = new object();

	public MiddlewareTypeDescriptor(Guid microService, Guid component)
	{
		MicroService = microService;
		Component = component;
	}
	public Guid Component { get; set; }
	public Guid MicroService { get; set; }

	public Type Type
	{
		get
		{
			if (_type is null && !_typeInitialized)
			{
				lock (_sync)
					if (_type is null && !_typeInitialized)
					{
						_typeInitialized = true;

						var text = Tenant.GetService<IDiscoveryService>().Configuration.Find(Component, Component) as IText;

						if (text is not null)
							_type = Tenant.GetService<ICompilerService>().ResolveType(MicroService, text, text.Configuration().ComponentName(), false);
					}
			}

			return _type;
		}
	}
}
