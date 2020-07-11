using System;

namespace TomPIT.Design
{
	public class ChangeBlob : IChangeBlob
	{
		public Guid Token { get; set; }
		public byte[] Content { get; set; }
		public string ContentType { get; set; }
		public string Syntax { get; set; }
		public string FileName { get; set; }
	}
}
