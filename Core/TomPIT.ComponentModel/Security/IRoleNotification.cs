namespace TomPIT.Security
{
	public interface IRoleNotification
	{
		void NotifyChanged(object sender, RoleEventArgs e);
	}
}
