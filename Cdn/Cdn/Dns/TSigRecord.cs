using System;

namespace TomPIT.Cdn.Dns
{
	internal class TSigRecord : IRecordData
	{
		private string _algorithm = string.Empty;
		private long _timeSigned = 0;
		private ushort _fudge = 0;
		private ushort _macSize = 0;
		private byte[] _mac = null;
		private ushort _originalId = 0;
		private ushort _error = 0;
		private ushort _otherLen = 0;
		private byte[] _otherData = null;
		public TSigRecord(DataBuffer buffer)
		{
			_algorithm = buffer.ReadDomainName();
			_timeSigned = buffer.ReadLongInt();
			_fudge = buffer.ReadShortUInt();
			_macSize = buffer.ReadShortUInt();
			_mac = buffer.ReadBytes(_macSize);
			_originalId = buffer.ReadShortUInt();
			_error = buffer.ReadShortUInt();
			_otherLen = buffer.ReadShortUInt();
			_otherData = buffer.ReadBytes(_otherLen);
		}
		public override string ToString()
		{
			return String.Format("Algorithm:{0} Signed Time:{1} Fudge Factor:{2} Mac:{3} Original ID:{4} Error:{5}\nOther Data:{6}",
				 _algorithm, _timeSigned, _fudge, _mac, _originalId, _error, _otherData);
		}

		public string Algorithm { get { return _algorithm; } }
		public long TimeSigned { get { return _timeSigned; } }
		public ushort Fudge { get { return _fudge; } }
		public byte[] Mac { get { return _mac; } }
		public ushort OriginalId { get { return _originalId; } }
		public ushort Error { get { return _error; } }
		public byte[] OtherData { get { return _otherData; } }
	}
}