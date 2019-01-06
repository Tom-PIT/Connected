namespace TomPIT.ComponentModel
{
	public interface IFeatureNotification
	{
		void NotifyChanged(object sender, FeatureEventArgs e);
	}
}
