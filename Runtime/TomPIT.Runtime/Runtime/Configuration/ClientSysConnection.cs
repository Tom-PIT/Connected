namespace TomPIT.Runtime.Configuration
{
	internal class ClientSysConnection : IClientSysConnection
	{
		public string Name { get; set; }
		public string Url { get; set; }
		public string AuthenticationToken { get; set; }
	}
}
