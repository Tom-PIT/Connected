namespace TomPIT.Runtime.Configuration
{
	internal class FileSystemDeployment : IClientSysFileSystemDeployment
	{
		public bool Enabled {get;set;}

		public string Path {get;set;}
	}
}
