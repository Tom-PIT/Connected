namespace TomPIT.Cdn.Dns
{
	public enum RecordType
	{
		None = 0,
		A = 1,
		NS = 2,
		CNAME = 5,
		SOA = 6,
		MB = 7,
		MG = 8,
		MR = 9,
		NULL = 10,
		WKS = 11,
		PTR = 12,
		HINFO = 13,
		MINFO = 14,
		MX = 15,
		TXT = 16,
		RP = 17,
		AFSDB = 18,
		X25 = 19,
		ISDN = 20,
		RT = 21,
		NSAP = 22,
		SIG = 24,
		KEY = 25,
		PX = 26,
		AAAA = 28,
		LOC = 29,
		SRV = 33,
		NAPTR = 35,
		KX = 36,
		A6 = 38,
		DNAME = 39,
		DS = 43,
		TKEY = 249,
		TSIG = 250,
		All = 255
	}

	internal class Question
	{
		private string _domain = string.Empty;
		private RecordType _recType = RecordType.All;
		private int _classType = 0;

		public Question(DataBuffer buffer)
		{
			_domain = buffer.ReadDomainName();
			_recType = (RecordType)buffer.ReadBEShortInt();
			_classType = buffer.ReadBEShortInt();
		}

		public string Domain { get { return _domain; } }
		public RecordType RecType { get { return _recType; } }
		public int ClassType { get { return _classType; } }
	}
}