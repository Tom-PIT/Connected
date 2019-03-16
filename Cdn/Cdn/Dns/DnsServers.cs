using System.Collections;

namespace TomPIT.Cdn.Dns
{
	internal class DnsServers : CollectionBase
	{
		public void Add(DnsServer server)
		{
			lock (List)
			{
				List.Add(server);
			}
		}

		public void Add(string host, int port)
		{
			lock (List)
			{
				List.Add(new DnsServer(host, port));
			}
		}

		public void Add(string host)
		{
			lock (List)
			{
				List.Add(new DnsServer(host, 25));
			}
		}

		public void Remove(int index)
		{
			if (index < Count || index >= 0)
			{
				lock (List)
				{
					List.RemoveAt(index);
				}
			}
		}

		public bool Exists(string host)
		{
			lock (List)
			{
				foreach (DnsServer server in List)
				{
					if (string.Compare(server.Host, host, true) == 0)
						return true;
				}
			}

			return false;
		}

		public DnsServer this[int index] { get { return (DnsServer)List[index]; } }
	}
}
