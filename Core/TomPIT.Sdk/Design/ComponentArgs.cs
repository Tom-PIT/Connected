using System;

namespace TomPIT.Design;
public class ComponentArgs : EventArgs
{
	public ComponentArgs(Guid microService, Guid component)
	{
		MicroService = microService;
		Component = component;
	}

	public Guid MicroService { get; }
	public Guid Component { get; }
}
