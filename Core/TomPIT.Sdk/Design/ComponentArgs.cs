using System;

namespace TomPIT.Design;
public class ComponentArgs : EventArgs
{
	public ComponentArgs(Guid microService, Guid component, string category)
	{
		MicroService = microService;
		Component = component;
		Category = category;
	}

	public Guid MicroService { get; }
	public Guid Component { get; }
	public string Category { get; }
}
