using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Routing;
using TomPIT.Runtime;
using TomPIT.Security;

namespace TomPIT.Models
{
	public class LoginModel : ShellModel
	{
		[Required]
		public string UserName { get; set; }
		public string Password { get; set; }
		public bool RememberMe { get; set; }
		[Required]
		public string ExistingPassword { get; set; }
		[Required]
		public string ConfirmPassword { get; set; }

		public IAuthenticationResult Authenticate()
		{
			return Tenant.GetService<IAuthorizationService>().Authenticate(UserName, Password);
		}

		public void ChangePassword()
		{
			var user = Tenant.GetService<IUserService>().Select(UserName);

			if (user == null)
				throw new TomPITException(SR.ErrUserNotFound);

			if (string.Compare(Password, ConfirmPassword, false) != 0)
				throw new TomPITException(SR.ValPasswordMatch);

			Tenant.GetService<IUserService>().ChangePassword(user.Token, ExistingPassword, Password);
		}

		public string ImageUrl { get { return Services.Routing.MapPath("~/Assets/Images/Shell/Login.jpg"); } }

		protected override void OnDatabinding()
		{
			Title = SR.Login;

			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.SingleTenant)
				Endpoint = Instance.Tenant.Url;

			Navigation.Breadcrumbs.Add(new Route
			{
				Text = SR.Login
			});
		}

		public List<ITenantDescriptor> QueryConnections()
		{
			return Shell.GetService<IConnectivityService>().QueryTenants();
		}

		public bool HasPasswordSet
		{
			get
			{
				var u = Tenant.GetService<IUserService>().Select(UserName);

				return u != null && u.HasPassword;
			}
		}

		public virtual void MapAuthenticate(JObject data)
		{
			UserName = data.Required<string>("user");
			Password = data.Optional("password", string.Empty);
			RememberMe = data.Optional("rememberMe", false);
		}

		public virtual void MapChangePassword(JObject data)
		{
			UserName = data.Required<string>("user");
			ExistingPassword = data.Optional("existing", string.Empty);
			Password = data.Required<string>("password");
			ConfirmPassword = data.Required<string>("confirm");
		}
	}
}
