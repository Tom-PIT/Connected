using System.Collections.Generic;

namespace TomPIT.Configuration
{
	public delegate void SettingChangedHandler(object sender, SettingEventArgs e);

	public interface ISettingService
	{
		event SettingChangedHandler SettingChanged;

		T GetValue<T>(string name, string type, string primaryKey);
		List<ISetting> Query();
		ISetting Select(string name, string type, string primaryKey);
		void Update(string name, string type, string primaryKey, object value);
	}
}
