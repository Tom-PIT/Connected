namespace TomPIT.Storage
{
	public interface IFileSystemProvider
	{ 
		string Id { get; }
		byte[] Read(FileSystemArgs e);
		void Write(FileSystemWriteArgs e);
	}
}
