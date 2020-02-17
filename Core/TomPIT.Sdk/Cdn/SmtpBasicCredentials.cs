namespace TomPIT.Cdn
{
	public class SmtpBasicCredentials : SmtpCredentials, ISmtpBasicCredentials
	{
		public string UserName { get; set; }

		public string Password { get; set; }
	}
}
