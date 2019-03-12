using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace TomPIT.Cdn.Dns
{
	public static class DnsResolve
	{
		private static DnsServers _dnsServers = null;
		private static ConcurrentDictionary<string, string> _cache = null;

		private static ConcurrentDictionary<string, string> cache
		{
			get
			{
				if (_cache == null)
					_cache = new ConcurrentDictionary<string, string>();

				return _cache;
			}
		}

		public static byte[] GetTxtRecords(string address)
		{
			if (DnsServers.Count == 0)
				return null;

			foreach (DnsServer server in DnsServers)
			{
				if (server.Host.Length > 3)
					return GetTxtRecords(address, server.Host, 53);
			}

			return null;
		}

		public static void Reset(string address)
		{
			if (cache.ContainsKey(address.Trim().ToLower()))
			{
				string v;

				cache.TryRemove(address.Trim().ToLower(), out v);
			}
		}
		public static string Resolve(string address)
		{
			if (string.IsNullOrWhiteSpace(address))
				return null;

			string key = address.Trim().ToLower();

			if (cache.ContainsKey(key))
			{
				string r;

				if (cache.TryGetValue(key, out r))
					return r;
			}

			if (DnsServers.Count == 0)
				return null;

			foreach (DnsServer i in DnsServers)
			{
				try
				{
					MxRecords records = GetMxRecords(address, i.Host, i.Port, 5000);

					if (records != null && records.Count > 0)
					{
						cache[key] = records.GetPrefered().Domain;

						return records.GetPrefered().Domain;
					}
				}
				catch { }

				List<string> cn = GetCNameRecords(address, i.Host, i.Port, 5000);

				try
				{
					if (cn != null && cn.Count > 0)
					{
						cache[key] = cn[0];

						return cn[0];
					}
				}
				catch { }
			}

			return null;
		}

		private static MxRecords GetMxRecords(string address, string host, int port, int timeout)
		{
			var r = new MxRecords();
			var q = new DnsQuery(IPAddress.Parse(host))
			{
				RecursiveQuery = true,
				DnsServer = { Port = port },
				Domain = address
			};

			DnsAnswer answer = q.QueryServer(RecordType.MX, timeout);

			if (answer != null)
			{
				foreach (Answer entry in answer.Answers)
				{
					var mxRecord = (MxRecord)entry.Data;

					r.Add(mxRecord.Domain, mxRecord.Preference);
				}
			}

			return r;
		}

		private static List<string> GetCNameRecords(string address, string host, int port, int timeout)
		{
			List<string> result = new List<string>();

			var q = new DnsQuery(IPAddress.Parse(host))
			{
				RecursiveQuery = true,
				DnsServer = { Port = port },
				Domain = address
			};

			DnsAnswer answer = q.QueryServer(RecordType.CNAME, timeout);

			if (answer != null)
			{
				foreach (Answer entry in answer.Answers)
				{
					CNameRecord rec = entry.Data as CNameRecord;

					if (rec != null)
						result.Add(rec.Domain);
				}
			}

			return result;
		}

		private static DnsServers DnsServers
		{
			get
			{
				if (_dnsServers == null)
				{
					_dnsServers = new DnsServers();

					IPGlobalProperties cp = IPGlobalProperties.GetIPGlobalProperties();
					NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

					if (nics == null)
						return _dnsServers;
					else
					{
						foreach (NetworkInterface adapter in nics)
						{
							IPInterfaceProperties properties = adapter.GetIPProperties();

							if (properties.DnsAddresses.Count > 0)
							{
								foreach (IPAddress ipAddress in properties.DnsAddresses)
									_dnsServers.Add(ipAddress.ToString(), 53);
							}
						}
					}
				}

				return _dnsServers;
			}
		}

		public static byte[] GetTxtRecords(string address, string host, int port)
		{
			byte[] header = { 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0 };
			string[] addressParts = address.Split('.');
			byte[] label = new byte[address.Length + 2];
			int pos = 0;

			foreach (string part in addressParts)
			{
				label[pos++] = System.Convert.ToByte(part.Length);

				foreach (char character in part)
					label[pos++] = System.Convert.ToByte(character);

				label[pos] = 0;
			}

			byte[] footer = { 0, 16, 0, 1 };
			byte[] query = new byte[header.Length + label.Length + footer.Length];

			header.CopyTo(query, 0);
			label.CopyTo(query, header.Length);
			footer.CopyTo(query, header.Length + label.Length);

			System.Net.IPEndPoint endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(host), 53);

			TimedUdpClient udpClient = new TimedUdpClient();

			udpClient.Connect(endPoint);
			udpClient.Send(query, query.Length);

			byte[] response2 = null;

			try
			{
				response2 = udpClient.Receive(ref endPoint);
			}
			catch (Exception)
			{
				udpClient.Close();

				throw new System.Exception("Can't connect to DNS server.");
			}

			pos = query.Length;

			GetLabelsByPos(response2, ref pos);

			pos += 7;

			int length = response2[pos];
			byte[] data = new byte[response2.Length - pos - 4];

			Array.Copy(response2, pos + 4, data, 0, response2.Length - pos - 4);

			return data;
		}

		private static string GetLabelsByPos(byte[] streamData, ref int pos)
		{
			int currentPos = pos;
			byte[] buffer = streamData;
			bool pointerFound = false;
			string temp = string.Empty, stringData = System.Text.Encoding.ASCII.GetString(streamData, 0, streamData.Length);

			byte labelLength = buffer[currentPos];

			while (labelLength != 0 && !pointerFound)
			{
				if ((labelLength & 192) == 192)
				{
					int newPointer;

					if (buffer[currentPos] == 192)
						newPointer = (buffer[currentPos] - 192) * 256 + buffer[currentPos + 1];
					else
						newPointer = buffer[currentPos + 1];

					temp += GetLabelsByPos(streamData, ref newPointer);
					temp += ".";

					currentPos += 2;

					pointerFound = true;
				}
				else
				{
					temp += stringData.Substring(currentPos + 1, labelLength) + ".";
					currentPos = currentPos + labelLength + 1;
					labelLength = buffer[currentPos];
				}
			}

			if (pointerFound)
				pos = currentPos;
			else
				pos = currentPos + 1;

			if (temp.Length > 0)
				return temp.TrimEnd('.');

			return temp;
		}
	}
}
