using System;

namespace TomPIT.Management
{
	public interface IConfigurationTypeDescriptor : IConfigurationDescriptor
	{
		Type SettingsType { get; }
	}
}
