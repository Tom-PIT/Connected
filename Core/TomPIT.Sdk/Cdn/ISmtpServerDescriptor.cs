namespace TomPIT.Cdn
{
	public interface ISmtpServerDescriptor
	{
		string Server { get; }
		string LocalDomain { get; }
	}
}
