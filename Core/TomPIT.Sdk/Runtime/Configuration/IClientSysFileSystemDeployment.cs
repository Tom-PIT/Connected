namespace TomPIT.Runtime.Configuration
{
	public interface IClientSysFileSystemDeployment
	{
		bool Enabled { get; }
		string Path { get; }
	}
}
