using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.Routing;

namespace TomPIT.Development.Models
{
	public abstract class DevelopmentModel : ShellModel
	{
		protected override void OnDatabinding()
		{
			base.OnDatabinding();

			Navigation.Links.Add(new Route
			{
				Category = "Quality Assurance",
				Text = "APIs",

				Url = MiddlewareDescriptor.Current.RouteUrl(this, "sys.apitest", null)
			});

			Navigation.Links.Add(new Route
			{
				Category = "Version Control",
				Text = "Version Control",

				Url = MiddlewareDescriptor.Current.RouteUrl(this, "sys.vc", null)
			});
		}
	}
}
