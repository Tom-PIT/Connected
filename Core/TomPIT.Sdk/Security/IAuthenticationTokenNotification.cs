namespace TomPIT.Security
{
	public interface IAuthenticationTokenNotification
	{
		void NotifyAuthenticationTokenChanged(object sender, AuthenticationTokenEventArgs e);
		void NotifyAuthenticationTokenRemoved(object sender, AuthenticationTokenEventArgs e);
	}
}
