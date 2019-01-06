using System;
using System.Collections.Generic;

namespace TomPIT.Configuration
{
	public delegate void SettingChangedHandler(object sender, SettingEventArgs e);

	public interface ISettingService
	{
		event SettingChangedHandler SettingChanged;

		T GetValue<T>(Guid resourceGroup, string name);
		List<ISetting> Query(Guid resourceGroup);
		ISetting Select(Guid resourceGroup, string name);
	}
}
