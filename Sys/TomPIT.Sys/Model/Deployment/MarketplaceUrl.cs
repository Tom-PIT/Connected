using TomPIT.Connectivity;

namespace TomPIT.Sys.Model.Deployment
{
	internal class MarketplaceUrl : ServerUrl
   {
      public MarketplaceUrl(string api, string operation)
      {
         BaseUrl = "http://rest.tompit.si/marketplace";
         Controller = api;
         Action = operation;
      }

      protected override string BaseUrl { get; set; }
   }
}
