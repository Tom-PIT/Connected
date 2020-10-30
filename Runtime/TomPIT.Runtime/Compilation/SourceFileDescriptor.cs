using System;

namespace TomPIT.Compilation
{
	public class SourceFileDescriptor
	{
		public string FileName { get; set; }
		public string Category { get; set; }
		public Guid Component { get; set; }
	}
}
