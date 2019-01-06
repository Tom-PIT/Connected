namespace TomPIT.Storage
{
	public interface IStorageNotification
	{
		void NotifyChanged(object sender, BlobEventArgs e);
		void NotifyRemoved(object sender, BlobEventArgs e);
		void NotifyAdded(object sender, BlobEventArgs e);
		void NotifyCommitted(object sender, BlobEventArgs e);
	}
}
