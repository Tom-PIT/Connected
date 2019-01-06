using System;

namespace TomPIT.Configuration
{
	public class SettingEventArgs : EventArgs
	{
		public SettingEventArgs(Guid resourceGroup, string name)
		{
			ResourceGroup = resourceGroup;
			Name = name;
		}

		public string Name { get; }
		public Guid ResourceGroup { get; }
	}
}
