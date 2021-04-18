using System;

namespace TomPIT.Environment
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
