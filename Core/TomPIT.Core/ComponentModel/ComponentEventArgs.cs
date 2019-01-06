using System;

namespace TomPIT.ComponentModel
{
	public class ComponentEventArgs : EventArgs
	{
		public ComponentEventArgs(Guid microService, Guid feature, Guid component)
		{
			MicroService = microService;
			Feature = feature;
			Component = component;
		}

		public Guid MicroService { get; }
		public Guid Feature { get; }
		public Guid Component { get; }
	}
}
