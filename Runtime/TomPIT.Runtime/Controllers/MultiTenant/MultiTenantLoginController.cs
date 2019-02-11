using Microsoft.AspNetCore.Mvc;
using TomPIT.Models;
using TomPIT.Models.MultiTenant;

namespace TomPIT.Controllers.MultiTenant
{
	public class MultiTenantLoginController : LoginController
	{
		protected override LoginModel CreateModel(Controller controller)
		{
			var r = new MultiTenantLoginModel();

			r.Initialize(controller, null);
			r.Databind();

			return r;
		}

		protected override string LoginView { get { return "~/Views/Shell/MultiTenant/MultiTenantLogin.cshtml"; } }
		protected override string LoginFormView { get { return "~/Views/Shell/MultiTenant/MultiTenantLoginForm.cshtml"; } }
		protected override string ChangePasswordView { get { return "~/Views/Shell/MultiTenant/MultiTenantChangePassword.cshtml"; } }
	}
}
