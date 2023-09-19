using LZ4;
using System;
using System.Linq;

namespace TomPIT.Storage
{
	internal static class BlobContentCompression
	{
		public static IBlobContent Decompress(Guid blob, byte[] compressed)
		{
			if (compressed is null || !compressed.Any())
				return new BlobContent(blob, null);

			return new BlobContent(blob, LZ4Codec.Unwrap(compressed));
		}

		private class BlobContent : IBlobContent
		{
			public BlobContent(Guid blob, byte[] content)
			{
				Blob = blob;
				Content = content;
			}

			public Guid Blob { get; }

			public byte[] Content { get; }
		}
	}
}
