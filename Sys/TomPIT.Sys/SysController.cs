using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace TomPIT.Sys
{
	[Authorize(AuthenticationSchemes = "TomPIT")]
	public class SysController : Controller
	{
		protected JObject FromBody()
		{
			return Request.Body.ToJObject();
		}

		protected T FromBody<T>()
		{
			return Request.Body.ToType<T>();
		}
	}
}
