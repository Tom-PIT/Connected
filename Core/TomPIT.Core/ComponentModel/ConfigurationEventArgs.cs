using System;

namespace TomPIT.ComponentModel
{
	public class ConfigurationEventArgs : EventArgs
	{
		public ConfigurationEventArgs(Guid microService, Guid component, string category)
		{
			Component = component;
			MicroService = microService;
			Category = category;
		}

		public Guid Component { get; }
		public Guid MicroService { get; }
		public string Category { get; }
	}
}
