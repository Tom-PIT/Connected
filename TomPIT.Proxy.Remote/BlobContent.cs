using System;
using TomPIT.Storage;

namespace TomPIT.Proxy.Remote
{
	internal class BlobContent : IBlobContent
	{
		public Guid Blob { get; set; }

		public byte[] Content { get; set; }
	}
}
