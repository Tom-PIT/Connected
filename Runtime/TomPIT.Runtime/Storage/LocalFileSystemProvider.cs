namespace TomPIT.Storage
{
	internal class LocalFileSystemProvider : IFileSystemProvider
	{
		public string Id => "Local";

		public byte[] Read(FileSystemArgs e)
		{

			//var credentials = e.Credentials.GetCredential();
			
			throw new System.NotImplementedException();
		}

		public void Write(FileSystemWriteArgs e)
		{
			throw new System.NotImplementedException();
		}
	}
}
