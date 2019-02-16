namespace TomPIT.ComponentModel
{
	public interface IMicroServiceNotification
	{
		void NotifyChanged(object sender, MicroServiceEventArgs e);
		void NotifyRemoved(object sender, MicroServiceEventArgs e);
		void NotifyMicroServiceStringChanged(object sender, MicroServiceStringEventArgs e);
		void NotifyMicroServiceStringRemoved(object sender, MicroServiceStringEventArgs e);
		void NotifyMicroServiceInstalled(object sender, MicroServiceEventArgs e);
	}
}
