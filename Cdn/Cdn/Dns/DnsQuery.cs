using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace TomPIT.Cdn.Dns
{
	public enum OpCode
	{
		StandardQuery = 0,
		InverseQuery = 1,
		StatusRequest = 2
	}

	internal class DnsQuery
	{
		private const int Port = 53;
		private const int MaxTries = 1;
		private const byte InClass = 1;

		private byte[] _query = null;
		private string _domain = string.Empty;
		private IPEndPoint _dnsServer = null;
		private bool _recursiveQuery = true;
		private Socket _reqSocket = null;
		private int _numTries = 0;
		private int _reqId = 0;

		public DnsQuery() { }

		public DnsQuery(string serverUrl)
		{
			IPHostEntry hostAddress = System.Net.Dns.GetHostEntry(serverUrl);

			if (hostAddress.AddressList.Length > 0)
				_dnsServer = new IPEndPoint(hostAddress.AddressList[0], Port);
			else
				throw new DnsQueryException("Invalid DNS Server Name Specified", null);
		}

		public DnsQuery(IPAddress dnsAddress)
		{
			_dnsServer = new IPEndPoint(dnsAddress, Port);
		}

		public DnsAnswer QueryServer(RecordType recType, int timeout)
		{
			if (_dnsServer == null)
				throw new DnsQueryException("There is no Dns server set in Dns Query Component", null);
			if (!ValidRecordType(recType))
				throw new DnsQueryException("Invalid Record Type submitted to Dns Query Component", null);

			DnsAnswer res = null;

			_numTries = 0;

			byte[] dnsResponse = new byte[512];
			Exception[] exceptions = new Exception[MaxTries];

			while (_numTries < MaxTries)
			{
				try
				{
					CreateDnsQuery(recType);

					_reqSocket = new Socket(_dnsServer.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
					{
						ReceiveTimeout = timeout
					};

					_reqSocket.SendTo(_query, _query.Length, SocketFlags.None, _dnsServer);
					_reqSocket.Receive(dnsResponse);

					if (dnsResponse[0] == _query[0] && dnsResponse[1] == _query[1])
						res = new DnsAnswer(dnsResponse);

					_numTries++;

					if (res.ReturnCode == ReturnCode.Success)
						return res;
				}
				catch (SocketException ex)
				{
					exceptions[_numTries] = ex;

					_numTries++;
					_reqId++;

					if (_numTries > MaxTries)
						throw new DnsQueryException("Failure Querying DNS Server", exceptions);
				}
				finally
				{
					_reqId++;
					_reqSocket.Close();

					Query = null;
				}

			}

			return res;
		}

		public DnsAnswer QueryServer(RecordType recType)
		{
			return QueryServer(recType, 5000);
		}

		private void CreateDnsQuery(RecordType recType)
		{
			List<Byte> queryBytes = new List<byte>();

			queryBytes.Add((byte)(_reqId >> 8));
			queryBytes.Add((byte)(_reqId));
			queryBytes.Add((byte)(((byte)OpCode.StandardQuery << 3) | (RecursiveQuery ? 0x01 : 0x00)));
			queryBytes.Add(0x00);
			queryBytes.Add(0x00);
			queryBytes.Add(0x01);

			for (int i = 0; i < 6; i++)
				queryBytes.Add(0x00);

			InsertDomainName(queryBytes, _domain);

			queryBytes.Add(0x00);
			queryBytes.Add((byte)recType);
			queryBytes.Add(0x00);
			queryBytes.Add(InClass);

			Query = queryBytes.ToArray();
		}
		private void InsertDomainName(List<Byte> data, string domain)
		{
			int length = 0;
			int pos = 0;

			while (pos < domain.Length)
			{
				int prev_pos = pos;

				pos = domain.IndexOf('.', pos);
				length = pos - prev_pos;

				if (length < 0)
					length = domain.Length - prev_pos;

				data.Add((byte)length);

				for (int i = 0; i < length; i++)
					data.Add((byte)domain[prev_pos++]);

				pos = prev_pos;
				pos++;
			}

			data.Add(0x00);
		}
		private bool ValidRecordType(RecordType t)
		{
			return (Enum.IsDefined(typeof(RecordType), t) || t == RecordType.All);
		}

		public byte[] Query
		{
			get { return _query; }
			set { _query = value; }
		}
		public string Domain
		{
			get { return _domain; }
			set
			{
				if (value.Length == 0 || value.Length > 255 ||
					 !Regex.IsMatch(value, @"^[a-z|A-Z|0-9|\-|_]{1,63}(\.[a-z|A-Z|0-9|\-]{1,63})+$"))
					throw new DnsQueryException("Invalid Domain Name", null);

				_domain = value;
			}
		}

		public IPEndPoint DnsServer
		{
			get { return _dnsServer; }
			set { _dnsServer = value; }
		}
		public bool RecursiveQuery
		{
			get { return _recursiveQuery; }
			set { _recursiveQuery = value; }
		}
	}
}
