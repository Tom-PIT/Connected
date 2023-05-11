namespace TomPIT.ComponentModel
{
	public interface IMicroServiceNotification
	{
		void NotifyChanged(object sender, MicroServiceEventArgs e);
		void NotifyRemoved(object sender, MicroServiceEventArgs e);
		void NotifyMicroServiceInstalled(object sender, MicroServiceInstallEventArgs e);
	}
}
