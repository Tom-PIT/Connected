using System;

namespace TomPIT.ComponentModel
{
	public class ComponentEventArgs : EventArgs
	{
		public ComponentEventArgs(Guid microService, Guid folder, Guid component)
		{
			MicroService = microService;
			Folder = folder;
			Component = component;
		}

		public Guid MicroService { get; }
		public Guid Folder { get; }
		public Guid Component { get; }
	}
}
