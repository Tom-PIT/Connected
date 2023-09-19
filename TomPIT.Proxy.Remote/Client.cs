using System;
using TomPIT.Environment;

namespace TomPIT.Proxy.Remote
{
	internal class Client : IClient
	{
		public string Token { get; set; }

		public string Name { get; set; }

		public DateTime Created { get; set; }

		public ClientStatus Status { get; set; }

		public string Type { get; set; }
	}
}
