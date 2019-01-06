using System.IO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TomPIT.Controllers
{
	public class ServerController : Controller
	{
		protected T FromBody<T>()
		{
			return Request.Body.ToType<T>();
		}

		protected JObject FromBody()
		{
			return Request.Body.ToJObject();
		}

		protected ActionResult AjaxRedirect(string url)
		{
			return Json(new { url });
		}
	}
}
