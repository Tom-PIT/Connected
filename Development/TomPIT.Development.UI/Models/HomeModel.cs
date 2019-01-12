using Microsoft.AspNetCore.Mvc;
using TomPIT.Design;

namespace TomPIT.Models
{
	public class HomeModel : DevelopmentModel
	{
		private DiscoveryModel _discovery = null;

		protected override void OnInitializing(ModelInitializeParams p)
		{
			Title = SR.DevelopmentEnvironment;
		}

		public string UrlIde(IUrlHelper helper, string microServiceUrl)
		{
			return helper.RouteUrl("ide", new
			{
				microService = microServiceUrl
			});
		}

		public DiscoveryModel Discovery
		{
			get
			{
				if (_discovery == null)
					_discovery = new DiscoveryModel(this);

				return _discovery;
			}
		}
	}
}
