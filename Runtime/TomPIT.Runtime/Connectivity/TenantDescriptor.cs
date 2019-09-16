namespace TomPIT.Connectivity
{
	internal class TenantDescriptor : ITenantDescriptor
	{
		public TenantDescriptor(string name, string url, string clientKey)
		{
			Name = name;
			Url = url;
			ClientKey = clientKey;
		}

		public string Name { get; }
		public string Url { get; }
		public string ClientKey { get; }
	}
}
