using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Controllers;

namespace TomPIT.App.Controllers
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
