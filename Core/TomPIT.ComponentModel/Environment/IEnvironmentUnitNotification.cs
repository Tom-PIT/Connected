namespace TomPIT.Environment
{
	public interface IEnvironmentUnitNotification
	{
		void NotifyChanged(object sender, EnvironmentUnitEventArgs e);
		void NotifyRemoved(object sender, EnvironmentUnitEventArgs e);
	}
}
