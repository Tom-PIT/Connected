using System;
using LZ4;

namespace TomPIT.Storage
{
	internal class BlobContent : IBlobContent
	{
		private byte[] _content;
		public Guid Blob { get; set; }
		private object _sync = new object();

		private bool Unpacked { get; set; }
		public byte[] Content
		{
			get
			{
				if (_content is null || _content.Length == 0)
					return _content;

				if (!Unpacked)
				{
					lock (_sync)
						if (!Unpacked)
						{
							_content = LZ4Codec.Unwrap(_content);

							Unpacked = true;
						}
				}

				return _content;
			}
			set => _content = value;
		}
	}
}
