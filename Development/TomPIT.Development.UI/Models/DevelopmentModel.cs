using TomPIT.Routing;

namespace TomPIT.Models
{
	public abstract class DevelopmentModel : ShellModel
	{
		protected override void OnDatabinding()
		{
			base.OnDatabinding();

			Navigation.Links.Add(new Route
			{
				Category = "Quality Assurance",
				Text = "Test suites",

				Url = this.RouteUrl("sys.testsuites", null)
			});

			Navigation.Links.Add(new Route
			{
				Category = "Quality Assurance",
				Text = "APIs",

				Url = this.RouteUrl("sys.apitest", null)
			});
		}
	}
}
