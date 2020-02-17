namespace TomPIT.Cdn
{
	public interface ISmtpBasicCredentials : ISmtpCredentials
	{
		string UserName { get; }
		string Password { get; }
	}
}
