using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Routing;

namespace TomPIT.Models
{
	public abstract class DevelopmentModel:ShellModel
	{
		protected override void OnDatabinding()
		{
			base.OnDatabinding();

			Navigation.Links.Add(new Route
			{
				Category = "Quality Assurance",
				Text = "API Test",

				Url = this.RouteUrl("sys.apitest", null)
			});
		}
	}
}
