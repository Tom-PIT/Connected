using System;

namespace TomPIT.Management
{
	public class ConfigurationTypeDescriptor : ConfigurationDescriptor, IConfigurationTypeDescriptor
	{
		public Type SettingsType { get; set; }
	}
}
