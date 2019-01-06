using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.Connectivity;
using TomPIT.Security;
using TomPIT.Services;

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
			return Connection.GetService<IAuthorizationService>().Authenticate(UserName, Password);
		}

		public void ChangePassword()
		{
			var user = Connection.GetService<IUserService>().Select(UserName);

			if (user == null)
				throw new TomPITException(SR.ErrUserNotFound);

			if (string.Compare(Password, ConfirmPassword, false) != 0)
				throw new TomPITException(SR.ValPasswordMatch);

			Connection.GetService<IUserService>().ChangePassword(user.Token, ExistingPassword, Password);
		}

		public string ImageUrl { get { return this.MapPath("~/Assets/Images/Shell/Login.jpg"); } }

		protected override void OnDatabinding()
		{
			Title = SR.Login;

			if (Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.SingleTenant)
				Endpoint = Instance.Connection.Url;
		}

		public List<ISysConnectionDescriptor> QueryConnections()
		{
			return Shell.GetService<IConnectivityService>().QueryConnections();
		}

		public bool HasPasswordSet
		{
			get
			{
				var u = Connection.GetService<IUserService>().Select(UserName);

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
