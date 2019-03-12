using System;

namespace TomPIT.Cdn.Dns
{
	internal class KeyRecord : IRecordData
	{
		private short _flags = 0;
		private byte _protocol = 0;
		private byte _algorithm = 0;
		private byte[] _publicKey = null;

		public KeyRecord(DataBuffer buffer, int length)
		{
			_flags = buffer.ReadShortInt();
			_protocol = buffer.ReadByte();
			_algorithm = buffer.ReadByte();
			_publicKey = buffer.ReadBytes(length - 4);
		}
		public override string ToString()
		{
			return String.Format("Flags:{0} Protocol:{1} Algorithm:{2} Public Key:{3}", _flags, _protocol, _algorithm, _publicKey);
		}

		public short Flags { get { return _flags; } }
		public byte Protocol { get { return _protocol; } }
		public byte Algorithm { get { return _algorithm; } }
		public byte[] PublicKey { get { return _publicKey; } }
	}
}
