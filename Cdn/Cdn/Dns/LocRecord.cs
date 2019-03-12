using System;

namespace TomPIT.Cdn.Dns
{
	internal class LocRecord : IRecordData
	{
		private short _version = 0;
		private short _size = 0;
		private short _horzPrecision = 0;
		private short _vertPrecision = 0;
		private long _lattitude = 0;
		private long _longitude = 0;
		private long _altitude = 0;

		public LocRecord(DataBuffer buffer)
		{
			_version = buffer.ReadShortInt();
			_size = buffer.ReadShortInt();
			_horzPrecision = buffer.ReadShortInt();
			_vertPrecision = buffer.ReadShortInt();
			_lattitude = buffer.ReadInt();
			_longitude = buffer.ReadInt();
			_altitude = buffer.ReadInt();
		}
		public override string ToString()
		{
			return String.Format("Version:{0} Size:{1} Horz Precision:{2} Veret Precision:{3} Lattitude:{4} Longitude:{5} Altitude:{6}",
				 _version, _size, _horzPrecision, _vertPrecision, _lattitude, _longitude, _altitude);
		}

		public short Version { get { return _version; } }
		public short Size { get { return _size; } }
		public short HorzPrecision { get { return _horzPrecision; } }
		public short VertPrecision { get { return _vertPrecision; } }
		public long Lattitude { get { return _lattitude; } }
		public long Longitude { get { return _longitude; } }
		public long Altitude { get { return _altitude; } }
	}
}
