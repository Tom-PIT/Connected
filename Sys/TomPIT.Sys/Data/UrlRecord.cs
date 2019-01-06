using TomPIT.Routing;

namespace TomPIT.Sys.Data
{
	public class UrlRecord : IUrlRecord
	{
		public UrlRecord(string id, string url)
		{
			Id = id;
			Url = url;
		}

		public string Id { get; }
		public string Url { get; }
	}
}
