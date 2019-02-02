namespace TomPIT.ComponentModel
{
	public interface IComponentNotification
	{
		void NotifyChanged(object sender, ComponentEventArgs e);
		void NotifyRemoved(object sender, ComponentEventArgs e);

		void NotifyChanged(object sender, ConfigurationEventArgs e);
		void NotifyAdded(object sender, ConfigurationEventArgs e);
		void NotifyRemoved(object sender, ConfigurationEventArgs e);

		void NotifyFolderChanged(object sender, FolderEventArgs e);
		void NotifyFolderRemoved(object sender, FolderEventArgs e);
	}
}
