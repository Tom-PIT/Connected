using System.Net;
using System.Text;

namespace TomPIT.Cdn.Dns
{
	internal class DataBuffer
	{
		public DataBuffer(byte[] data) : this(data, 0) { }
		public DataBuffer(byte[] data, int position)
		{
			Data = data;
			Position = position;
		}

		public byte Next
		{
			get
			{
				return Data[Position];
			}
		}

		public byte ReadByte()
		{
			return Data[Position++];
		}

		public short ReadShortInt()
		{
			return (short)(ReadByte() | ReadByte() << 8);
		}

		public short ReadBEShortInt()
		{
			return (short)(ReadByte() << 8 | ReadByte());
		}

		public ushort ReadShortUInt()
		{
			return (ushort)(ReadByte() | ReadByte() << 8);
		}

		public ushort ReadBEShortUInt()
		{
			return (ushort)(ReadByte() << 8 | ReadByte());
		}

		public int ReadInt()
		{
			return (ReadBEShortUInt() << 16 | ReadBEShortUInt());
		}

		public uint ReadUInt()
		{
			return (uint)(ReadBEShortUInt() << 16 | ReadBEShortUInt());
		}

		public long ReadLongInt()
		{
			return ReadInt() | ReadInt() << 32;
		}

		public string ReadDomainName()
		{
			return ReadDomainName(1);
		}

		public string ReadDomainName(int depth)
		{
			var domain = new StringBuilder();
			var length = ReadByte();

			while (length != 0)
			{
				if ((length & 0xc0) == 0xc0)
				{
					var posReference = ((length & 0x3f) << 8 | ReadByte());
					var oldPosition = Position;

					Position = posReference;
					domain.Append(ReadDomainName(depth + 1));
					Position = oldPosition;

					return domain.ToString();
				}
				else
				{
					for (var i = 0; i < length; i++)
						domain.Append((char)ReadByte());
				}

				if (Next != 0)
					domain.Append('.');

				length = ReadByte();
			}

			return domain.ToString();
		}

		public IPAddress ReadIPAddress()
		{
			var address = new byte[4];

			for (var i = 0; i < 4; i++)
				address[i] = ReadByte();

			return new IPAddress(address);
		}

		public IPAddress ReadIPv6Address()
		{
			var address = new byte[16];

			for (var i = 0; i < 16; i++)
				address[i] = ReadByte();

			return new IPAddress(address);
		}

		public byte[] ReadBytes(int length)
		{
			var res = new byte[length];

			for (var i = 0; i < length; i++)
				res[i] = ReadByte();

			return res;
		}

		public string ReadCharString()
		{
			var length = ReadByte();
			var txt = new StringBuilder();

			for (var i = 0; i < length; i++)
				txt.Append((char)ReadByte());

			return txt.ToString();
		}

		public int Position { get; set; } = 0;
		private byte[] Data { get; set; } = null;
	}
}
