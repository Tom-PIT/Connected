namespace TomPIT.Environment
{
	public interface IClientNotification
	{
		void NotifyChanged(object sender, ClientEventArgs e);
	}
}
