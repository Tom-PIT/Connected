namespace TomPIT.Configuration
{
	public interface ISettingNotification
	{
		void NotifyChanged(object sender, SettingEventArgs e);
		void NotifyRemoved(object sender, SettingEventArgs e);
	}
}
