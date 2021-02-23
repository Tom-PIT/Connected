using TomPIT.Storage;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareStorageFileSystem
	{
		void Write(FileSystemWriteArgs e);
		void Delete(FileSystemArgs e);
		byte[] Read(FileSystemArgs e);
		void Copy(FileSystemMoveArgs e);
		void Move(FileSystemMoveArgs e);
	}
}
