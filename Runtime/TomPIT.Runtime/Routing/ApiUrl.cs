using TomPIT.Connectivity;

namespace TomPIT.Routing
{
	internal class ApiUrl : ServerUrl
	{
		public ApiUrl(string baseUrl, string microService, string api, string operation)
		{
			BaseUrl = string.Format("{0}/{1}", baseUrl, microService);
			Controller = api;
			Action = operation;
		}

		protected override string BaseUrl { get; set; }
	}
}
