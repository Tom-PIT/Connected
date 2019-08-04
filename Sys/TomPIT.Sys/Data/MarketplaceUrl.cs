using TomPIT.Connectivity;

namespace TomPIT.Sys.Data
{
    internal class MarketplaceUrl : ServerUrl
    {
        public MarketplaceUrl(string api, string operation)
        {
            BaseUrl = "http://rest.tompit.net/marketplace";
            Controller = api;
            Action = operation;
        }

        protected override string BaseUrl { get; set; }
    }
}
