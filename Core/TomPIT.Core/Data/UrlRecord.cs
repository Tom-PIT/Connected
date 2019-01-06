using TomPIT.Routing;

namespace TomPIT.Data
{
	public class UrlRecord : IUrlRecord
	{
		public UrlRecord()
		{

		}

		public UrlRecord(string id, string url)
		{
			Id = id;
			Url = url;
		}

		public string Url { get; set; }
		public string Id { get; set; }
	}
}
