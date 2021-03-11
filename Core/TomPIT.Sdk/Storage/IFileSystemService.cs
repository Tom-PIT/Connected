namespace TomPIT.Storage
{
	public interface IFileSystemService
	{
		byte[] Read(FileSystemArgs e);
		void Write(FileSystemWriteArgs e);
		void Delete(FileSystemArgs e);
		void Copy(FileSystemMoveArgs e);
		void Move(FileSystemMoveArgs e);
	}
}
