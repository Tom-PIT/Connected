using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TomPIT.Controllers
{
	[AllowAnonymous]
	public class IoTController : ServerController
	{
		[HttpPost]
		public IActionResult GetValue()
		{
			return null;
			//var value = Instance.GetService<IIoTService>().SelectState();
			//return Json(JsonConvert.SerializeObject(m.Invoke<object>(m.QualifierName, m.Body)));
		}
	}
}
