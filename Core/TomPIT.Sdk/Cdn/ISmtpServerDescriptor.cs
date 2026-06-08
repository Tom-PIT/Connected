namespace TomPIT.Cdn
{
	public interface ISmtpServerDescriptor
	{
		string Server { get; }
		string LocalDomain { get; }
		int Port { get; }
	}
}
