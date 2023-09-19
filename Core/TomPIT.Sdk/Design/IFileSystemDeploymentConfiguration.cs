namespace TomPIT.Design
{
	public interface IFileSystemDeploymentConfiguration
	{
		bool Enabled { get; }
		string Path { get; }
	}
}
