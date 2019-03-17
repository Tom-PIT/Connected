using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TomPIT.Services;

namespace TomPIT.BigData.Controllers
{
	internal class ApiHandler : ExecutionContext
	{
		public ApiHandler(HttpContext context)
		{
			var ms = context.GetRouteValue("microService");
			var api = context.GetRouteValue("api");
		}

		public void ProcessRequest()
		{

		}
	}
}
