using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using TomPIT.BigData.Controllers;

namespace TomPIT.BigData.Configuration
{
	internal static class Routing
	{
		public static void Register(IRouteBuilder builder)
		{
			builder.MapRoute("{microService}/{api}", (t) =>
			{
				var handler = new ApiHandler(t);

				handler.ProcessRequest();

				return Task.CompletedTask;
			});
		}
	}
}
