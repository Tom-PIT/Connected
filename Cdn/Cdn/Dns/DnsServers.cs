using System.Collections;

namespace TomPIT.Cdn.Dns
{
	internal class DnsServers : CollectionBase
	{
		public static DnsServers operator +(DnsServers first, DnsServers second)
		{
			DnsServers newServers = first;

			foreach (DnsServer server in second)
				newServers.Add(server);

			return newServers;
		}

		public void Add(DnsServer server)
		{
			List.Add(server);
		}

		public void Add(string host, int port)
		{
			List.Add(new DnsServer(host, port));
		}

		public void Add(string host)
		{
			List.Add(new DnsServer(host, 25));
		}

		public void Remove(int index)
		{
			if (index < Count || index >= 0)
				List.RemoveAt(index);
		}

		public DnsServer this[int index] { get { return (DnsServer)List[index]; } }
	}
}
