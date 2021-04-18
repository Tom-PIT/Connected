using System;

namespace TomPIT.Storage
{
	public class FileSystemArgs : EventArgs
	{
		public string  UserName { get; set; }
		public string Password { get; set; }
		public string Path { get; set; }
	}
}
