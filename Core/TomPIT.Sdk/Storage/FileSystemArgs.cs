using System;

namespace TomPIT.Storage
{
	public class FileSystemArgs : EventArgs
	{
		public IFileSystemCredentials Credentials { get; set; }
		public string Path { get; set; }
	}
}
