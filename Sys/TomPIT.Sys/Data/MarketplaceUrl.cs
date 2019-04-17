using TomPIT.Connectivity;

namespace TomPIT.Sys.Data
{
    internal class MarketplaceUrl : ServerUrl
    {
        public MarketplaceUrl(string api, string operation)
        {
            BaseUrl = "http://192.168.10.6/marketplace/rest/marketplace";
            Controller = api;
            Action = operation;
        }

        protected override string BaseUrl { get; set; }
    }
}
