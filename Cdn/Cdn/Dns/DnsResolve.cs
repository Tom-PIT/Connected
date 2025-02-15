﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace TomPIT.Cdn.Dns
{
	public static class DnsResolve
	{
		private static DnsServers _dnsServers = null;
		private static ConcurrentDictionary<string, DomainDescriptor> _cache = null;

		private static ConcurrentDictionary<string, DomainDescriptor> cache
		{
			get
			{
				if (_cache == null)
					_cache = new ConcurrentDictionary<string, DomainDescriptor>();

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
			cache.TryRemove(address.Trim().ToLower(), out _);
		}
		public static DomainDescriptor Resolve(string address)
		{
			if (string.IsNullOrWhiteSpace(address))
				return null;

			string key = address.Trim().ToLower();

			if (cache.ContainsKey(key))
			{
				if (cache.TryGetValue(key, out DomainDescriptor descriptor))
					return descriptor;
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
						var descriptor = new DomainDescriptor
						{
							Primary = records.GetPrefered().Domain,
							Secondary = ResolveHost(records.GetPrefered().Domain)
						};

						cache[key] = descriptor;

						return descriptor;
					}
				}
				catch { }

				List<string> cn = GetCNameRecords(address, i.Host, i.Port, 5000);

				try
				{
					if (cn != null && cn.Count > 0)
					{
						var descriptor = new DomainDescriptor
						{
							Primary = cn[0]
						};

						cache[key] = descriptor;

						return descriptor;
					}
				}
				catch { }
			}

			return null;
		}

		private static string ResolveHost(string domain)
		{
			try
			{
				var entry = System.Net.Dns.GetHostEntry(domain);

				if (entry.AddressList.Length == 0)
					return domain;

				var host = System.Net.Dns.GetHostEntry(entry.AddressList[0]);

				return host.HostName;
			}
			catch
			{
				return domain;
			}
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

					lock (_dnsServers)
					{
						if (_dnsServers.Count > 0)
							return _dnsServers;

						var cp = IPGlobalProperties.GetIPGlobalProperties();
						var nics = NetworkInterface.GetAllNetworkInterfaces();

						if (nics == null)
							return _dnsServers;
						else
						{
							foreach (var adapter in nics)
							{
								var properties = adapter.GetIPProperties();

								if (properties.DnsAddresses.Count > 0)
								{
									foreach (var ipAddress in properties.DnsAddresses)
									{
										var ip = ipAddress.ToString();

										if (string.IsNullOrWhiteSpace(ip) || _dnsServers.Exists(ip))
											continue;

										_dnsServers.Add(ip, 53);
									}
								}
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
