using TomPIT.Storage;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareStorageFileSystem : MiddlewareObject, IMiddlewareStorageFileSystem
	{
		public MiddlewareStorageFileSystem(IMiddlewareContext context) : base(context)
		{
		}

		public void Copy(FileSystemMoveArgs e)
		{
			Context.Tenant.GetService<IFileSystemService>().Copy(e);
		}

		public void Delete(FileSystemArgs e)
		{
			Context.Tenant.GetService<IFileSystemService>().Delete(e);
		}

		public void Move(FileSystemMoveArgs e)
		{
			Context.Tenant.GetService<IFileSystemService>().Move(e);
		}

		public byte[] Read(FileSystemArgs e)
		{
			return Context.Tenant.GetService<IFileSystemService>().Read(e);
		}

		public void Write(FileSystemWriteArgs e)
		{
			Context.Tenant.GetService<IFileSystemService>().Write(e);
		}
	}
}
