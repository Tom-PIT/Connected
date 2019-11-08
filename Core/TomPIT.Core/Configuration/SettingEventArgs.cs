using System;

namespace TomPIT.Configuration
{
	public class SettingEventArgs : EventArgs
	{
		public SettingEventArgs()
		{

		}
		public SettingEventArgs(Guid resourceGroup, string name)
		{
			ResourceGroup = resourceGroup;
			Name = name;
		}

		public string Name { get; set; }
		public Guid ResourceGroup { get; set; }
	}
}
