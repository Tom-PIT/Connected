using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn.Data;
using TomPIT.Controllers;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.Cdn.Controllers
{
	public class DataController : ServerController
	{
		[HttpPost]
		public IActionResult Notify()
		{
			var body = FromBody();

			MiddlewareDescriptor.Current.Tenant.GetService<IDataHubService>().Notify(body.Required<string>("endpoint"), Serializer.Serialize(body.Optional<JObject>("arguments", null)));

			return new EmptyResult();
		}
	}
}
