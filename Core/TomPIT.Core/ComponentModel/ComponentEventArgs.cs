using System;

namespace TomPIT.ComponentModel
{
	public class ComponentEventArgs : EventArgs
	{
		public ComponentEventArgs()
		{

		}
		public ComponentEventArgs(Guid microService, Guid folder, Guid component, string nameSpace, string category, string name)
		{
			MicroService = microService;
			Folder = folder;
			Component = component;
			Category = category;
			NameSpace = nameSpace;
			Name = name;
		}

		public string Name { get; set; }
		public string NameSpace { get; set; }
		public Guid MicroService { get; set; }
		public Guid Folder { get; set; }
		public Guid Component { get; set; }
		public string Category { get; set; }
	}
}
