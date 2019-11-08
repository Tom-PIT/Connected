namespace TomPIT.Security
{
	public interface IUserNotification
	{
		void NotifyChanged(object sender, UserEventArgs e);
	}
}
