namespace TomPIT.Design
{
	internal class DeploymentConfiguration : IDeploymentConfiguration
	{
		public DeploymentConfiguration()
		{
			FileSystem = new FileSystemDeploymentConfiguration();
		}

		public IFileSystemDeploymentConfiguration FileSystem { get; }
	}
}
