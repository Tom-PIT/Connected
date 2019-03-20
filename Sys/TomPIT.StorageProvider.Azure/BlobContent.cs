using System;
using TomPIT.Storage;

namespace TomPIT.StorageProvider.Azure
{
	internal class BlobContent : IBlobContent
	{
		public byte[] Content { get; set; }
		public Guid Blob { get; set; }
	}
}
