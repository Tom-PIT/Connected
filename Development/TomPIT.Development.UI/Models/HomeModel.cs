using Microsoft.AspNetCore.Mvc;
using TomPIT.Design;
using TomPIT.Routing;

namespace TomPIT.Models
{
	public class HomeModel : ShellModel
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

		protected override void OnDatabinding()
		{
			base.OnDatabinding();

			Navigation.Links.Add(new Route
			{
				Category = "Testing",
				Text = "API",
				Url = this.MapPath("~/sys/apitest")
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
