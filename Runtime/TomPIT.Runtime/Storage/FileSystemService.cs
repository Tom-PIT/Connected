using System.IO;
using System.Net;
using TomPIT.Connectivity;
using TomPIT.Exceptions;

namespace TomPIT.Storage
{
	internal class FileSystemService : TenantObject, IFileSystemService
	{
		public void Copy(FileSystemMoveArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.Path))
				throw new RuntimeException(SR.ErrPathNotSet);

			if (string.IsNullOrWhiteSpace(e.NewPath))
				throw new RuntimeException(SR.ErrNewPathNotSet);

			using var connection = new NetworkConnection(e.Path, new NetworkCredential(e.UserName, e.Path));
			
			File.Copy(e.Path, e.NewPath);
		}

		public void Delete(FileSystemArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.Path))
				throw new RuntimeException(SR.ErrPathNotSet);

			using var connection = new NetworkConnection(e.Path, new NetworkCredential(e.UserName, e.Path));

			File.Delete(e.Path);
		}

		public void Move(FileSystemMoveArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.Path))
				throw new RuntimeException(SR.ErrPathNotSet);

			if (string.IsNullOrWhiteSpace(e.NewPath))
				throw new RuntimeException(SR.ErrNewPathNotSet);

			using var connection = new NetworkConnection(e.Path, new NetworkCredential(e.UserName, e.Path));

			File.Move(e.Path, e.NewPath);
		}

		public byte[] Read(FileSystemArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.Path))
				throw new RuntimeException(SR.ErrPathNotSet);

			using var connection = new NetworkConnection(e.Path, new NetworkCredential(e.UserName, e.Path));

			return File.ReadAllBytes(e.Path);
		}

		public void Write(FileSystemWriteArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.Path))
				throw new RuntimeException(SR.ErrPathNotSet);

			var directoryName = Path.GetDirectoryName(e.Path);

			if (!Directory.Exists(directoryName))
				Directory.CreateDirectory(directoryName);

			File.WriteAllBytes(e.Path, e.Content);
		}
	}
}
