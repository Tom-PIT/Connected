using System;

namespace TomPIT.ComponentModel
{
	public class ComponentEventArgs : EventArgs
	{
		public ComponentEventArgs()
		{

		}
		public ComponentEventArgs(Guid microService, Guid folder, Guid component, string category)
		{
			MicroService = microService;
			Folder = folder;
			Component = component;
			Category = category;
		}

		public Guid MicroService { get; set; }
		public Guid Folder { get; set; }
		public Guid Component { get; set; }
		public string Category { get; set; }
	}
}
