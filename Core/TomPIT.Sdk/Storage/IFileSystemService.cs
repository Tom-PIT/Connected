namespace TomPIT.Storage
{
	public interface IFileSystemService
	{
		byte[] Read(FileSystemArgs e);
		void Write(FileSystemWriteArgs e);
	}
}
