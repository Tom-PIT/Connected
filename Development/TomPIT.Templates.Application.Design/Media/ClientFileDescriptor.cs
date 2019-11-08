using System;

namespace TomPIT.MicroServices.Design.Media
{
	internal class ClientFileDescriptor
	{
		public DateTime Modified { get; set; }
		public bool HasSubDirectories { get; set; }
		public bool IsDirectory { get; set; }
		public string Name { get; set; }
		public long Size { get; set; }
		public string Icon { get; set; }
	}
}
