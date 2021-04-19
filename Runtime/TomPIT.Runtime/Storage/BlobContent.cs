using System;
using LZ4;

namespace TomPIT.Storage
{
	internal class BlobContent : IBlobContent
	{
		private byte[] _content;
		public Guid Blob { get; set; }

		private bool Unpacked { get; set; }
		public byte[] Content
		{
			get
			{
				if (_content is null || _content.Length == 0)
					return _content;

				if (!Unpacked)
				{
					lock (_content)
						if (!Unpacked)
						{
							Unpacked = true;

							_content = LZ4Codec.Unwrap(_content);
						}
				}

				return _content;
			}
			set => _content = value;
		}
	}
}
