using System;

namespace TomPIT.Storage
{
	internal class BlobContent : IBlobContent
	{
		public Guid Blob { get; set; }
		public byte[] Content { get; set; }
	}
}
