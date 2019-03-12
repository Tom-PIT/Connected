using System;

namespace TomPIT.Cdn.Dns
{
	internal class DsRecord : IRecordData
	{
		private short _key = 0;
		private byte _algorithm = 0;
		private byte _digestType = 0;
		private byte[] _digest = null;

		public DsRecord(DataBuffer buffer, int length)
		{
			_key = buffer.ReadShortInt();
			_algorithm = buffer.ReadByte();
			_digestType = buffer.ReadByte();
			_digest = buffer.ReadBytes(length - 4);
		}
		public override string ToString()
		{
			return String.Format("Key:{0} Algorithm:{1} DigestType:{2} Digest:{3}", _key, _algorithm, _digestType, _digest);
		}

		public short Key { get { return _key; } }
		public byte Algorithm { get { return _algorithm; } }
		public byte DigestType { get { return _digestType; } }
		public byte[] Digest { get { return _digest; } }
	}
}
