using System;

namespace TomPIT.Cdn.Dns
{
	internal class DnsEntry
	{
		public DnsEntry(DataBuffer buffer)
		{
			try
			{
				Domain = buffer.ReadDomainName();

				var b = buffer.ReadByte();

				RecType = (RecordType)buffer.ReadShortInt();
				ClassType = buffer.ReadShortInt();
				Ttl = buffer.ReadInt();

				var length = buffer.ReadByte();

				switch (RecType)
				{
					case RecordType.A:
						Data = new ARecord(buffer);
						break;
					case RecordType.NS:
						Data = new NsRecord(buffer);
						break;
					case RecordType.CNAME:
						Data = new CNameRecord(buffer);
						break;
					case RecordType.SOA:
						Data = new SoaRecord(buffer);
						break;
					case RecordType.MB:
						Data = new MbRecord(buffer);
						break;
					case RecordType.MG:
						Data = new MgRecord(buffer);
						break;
					case RecordType.MR:
						Data = new MrRecord(buffer);
						break;
					case RecordType.NULL:
						Data = new NullRecord(buffer, length);
						break;
					case RecordType.WKS:
						Data = new WksRecord(buffer, length);
						break;
					case RecordType.PTR:
						Data = new PtrRecord(buffer);
						break;
					case RecordType.HINFO:
						Data = new HInfoRecord(buffer, length);
						break;
					case RecordType.MINFO:
						Data = new MInfoRecord(buffer);
						break;
					case RecordType.MX:
						Data = new MxRecord(buffer);
						break;
					case RecordType.TXT:
						Data = new TxtRecord(buffer, length);
						break;
					case RecordType.RP:
						Data = new RpRecord(buffer);
						break;
					case RecordType.AFSDB:
						Data = new AfsdbRecord(buffer);
						break;
					case RecordType.X25:
						Data = new X25Record(buffer);
						break;
					case RecordType.ISDN:
						Data = new IsdnRecord(buffer);
						break;
					case RecordType.RT:
						Data = new RtRecord(buffer);
						break;
					case RecordType.NSAP:
						Data = new NsapRecord(buffer, length);
						break;
					case RecordType.SIG:
						Data = new SigRecord(buffer, length);
						break;
					case RecordType.KEY:
						Data = new KeyRecord(buffer, length);
						break;
					case RecordType.PX:
						Data = new PxRecord(buffer);
						break;
					case RecordType.AAAA:
						Data = new AaaaRecord(buffer);
						break;
					case RecordType.LOC:
						Data = new LocRecord(buffer);
						break;
					case RecordType.SRV:
						Data = new SrvRecord(buffer);
						break;
					case RecordType.NAPTR:
						Data = new NaptrRecord(buffer);
						break;
					case RecordType.KX:
						Data = new KxRecord(buffer);
						break;
					case RecordType.A6:
						Data = new A6Record(buffer);
						break;
					case RecordType.DNAME:
						Data = new DNameRecord(buffer);
						break;
					case RecordType.DS:
						Data = new DsRecord(buffer, length);
						break;
					case RecordType.TKEY:
						Data = new TKeyRecord(buffer);
						break;
					case RecordType.TSIG:
						Data = new TSigRecord(buffer);
						break;
					default:
						throw new DnsQueryException("Invalid DNS Record Type in DNS Response", null);
				}
			}
			catch (Exception ex)
			{
				Data = new ExceptionRecord(ex.Message);
				throw;
			}

		}

		public string Domain { get; } = string.Empty;
		public RecordType RecType { get; } = RecordType.All;
		public int ClassType { get; } = 0;
		public int Ttl { get; } = 0;
		public IRecordData Data { get; set; } = null;
	}
}
