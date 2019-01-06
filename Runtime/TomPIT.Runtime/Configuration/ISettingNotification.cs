namespace TomPIT.Configuration
{
	public interface ISettingNotification
	{
		void NotifyChanged(object sender, SettingEventArgs e);
	}
}
