using System;

namespace TomPIT.ComponentModel
{
	public class ConfigurationEventArgs : EventArgs
	{
		public ConfigurationEventArgs()
		{

		}
		public ConfigurationEventArgs(Guid microService, Guid component, string category)
		{
			Component = component;
			MicroService = microService;
			Category = category;
		}

		public Guid Component { get; set; }
		public Guid MicroService { get; set; }
		public string Category { get; set; }
	}
}
