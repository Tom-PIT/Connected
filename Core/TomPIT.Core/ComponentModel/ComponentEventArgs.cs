using System;

namespace TomPIT.ComponentModel
{
	public class ComponentEventArgs : EventArgs
	{
		public ComponentEventArgs(Guid microService, Guid folder, Guid component, string category)
		{
			MicroService = microService;
			Folder = folder;
			Component = component;
			Category = category;
		}

		public Guid MicroService { get; }
		public Guid Folder { get; }
		public Guid Component { get; }
		public string Category { get; }
	}
}
