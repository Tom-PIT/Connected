namespace TomPIT.Cdn
{
	public class SmtpServerDescriptor : ISmtpServerDescriptor
	{
		public string Server { get; set; }

		public string LocalDomain { get; set; }
	}
}
