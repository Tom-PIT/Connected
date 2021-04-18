using System;

namespace TomPIT.Design
{
	internal class ComponentImageBlob : IComponentImageBlob
	{
		public Guid Token { get; set; }
		public string ContentType { get; set; }
		public string FileName { get; set; }
		public string PrimaryKey { get; set; }
		public string Topic { get; set; }
		public int Type { get; set; }
		public int Version { get; set; }
		public byte[] Content { get; set; }
	}
}
