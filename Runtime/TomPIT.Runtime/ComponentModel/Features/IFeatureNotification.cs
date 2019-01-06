namespace TomPIT.ComponentModel.Features
{
	public interface IFeatureNotification
	{
		void NotifyChanged(object sender, FeatureEventArgs e);
	}
}
