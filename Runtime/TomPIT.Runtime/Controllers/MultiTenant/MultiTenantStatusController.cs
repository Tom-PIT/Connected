using Microsoft.AspNetCore.Mvc;
using TomPIT.Models.MultiTenant;

namespace TomPIT.Controllers.MultiTenant
{
	public class MultiTenantStatusController : StatusController
	{

		protected override IActionResult CreateLoginResult()
		{
			var model = new MultiTenantLoginModel();

			model.Initialize(this, null);
			model.Databind();

			return View("~/Views/Shell/MultiTenant/MultiTenantLogin.cshtml", model);
		}
	}
}
