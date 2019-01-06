namespace TomPIT.Connectivity
{
	internal class SysConnectionDescriptor : ISysConnectionDescriptor
	{
		public SysConnectionDescriptor(string name, string url, string clientKey)
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
