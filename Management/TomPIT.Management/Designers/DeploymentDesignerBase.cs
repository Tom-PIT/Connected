using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Deployment;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Dom;
using TomPIT.Management.Deployment;

namespace TomPIT.Management.Designers
{
	public abstract class DeploymentDesignerBase<T> : DomDesigner<T>, ISignupDesigner where T : IDomElement
	{
		private List<ICountry> _countries = null;

		public DeploymentDesignerBase(T element) : base(element)
		{
		}

		public override object ViewModel => this;
		public Guid PublisherKey { get; private set; }

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "signup", true) == 0)
				return Result.ViewResult(this, "~/Views/Ide/Designers/DeploymentSignup.cshtml");
			else if (string.Compare(action, "login", true) == 0)
				return Result.ViewResult(this, "~/Views/Ide/Designers/DeploymentLogin.cshtml");
			else if (string.Compare(action, "signupcreate", true) == 0)
				return SignUp(data);
			else if (string.Compare(action, "checkConfirmation", true) == 0)
				return CheckConfirmation(data);
			else if (string.Compare(action, "authenticate", true) == 0)
				return Login(data);
			else if (string.Compare(action, "logoff", true) == 0)
				return Logoff(data);

			return base.OnAction(data, action);
		}

		public List<ICountry> Countries
		{
			get
			{
				if (_countries == null)
					_countries = Environment.Context.Tenant.GetService<IDeploymentService>().QueryCountries();

				return _countries;
			}
		}

		public IAccount Account { get { return Environment.Context.Tenant.GetService<IDeploymentService>().Account; } }
		protected abstract string MainView { get; }

		private IDesignerActionResult Logoff(JObject data)
		{
			Environment.Context.Tenant.GetService<IDeploymentService>().LogOut();

			return Result.ViewResult(this, LoginView);
		}

		private IDesignerActionResult Login(JObject data)
		{
			Environment.Context.Tenant.GetService<IDeploymentService>().LogIn(data.Required<string>("user"), data.Required<string>("password"));

			if (Environment.Context.Tenant.GetService<IDeploymentService>().IsLogged)
			{
				var r = Result.ViewResult(ViewModel, MainView);

				r.ResponseHeaders.Add("viewType", "package");

				return r;
			}
			else
			{
				var r = Result.ViewResult(ViewModel, "~/Views/Ide/Designers/DeploymentLoginForm.cshtml");

				r.ResponseHeaders.Add("viewType", "login");

				return r;
			}
		}

		private IDesignerActionResult SignUp(JObject data)
		{
			var company = data.Required<string>("company");
			var country = data.Required<string>("country");
			var website = data.Optional("website", string.Empty);
			var firstName = data.Required<string>("firstName");
			var lastName = data.Required<string>("lastName");
			var password = data.Required<string>("password");
			var phone = data.Optional("phone", string.Empty);
			var email = data.Required<string>("email");

			PublisherKey = Environment.Context.Tenant.GetService<IDeploymentService>().SignUp(company, firstName, lastName, password, email, country, phone, website);

			return Result.ViewResult(ViewModel, "~/Views/Ide/Designers/DeploymentInbox.cshtml");
		}

		private IDesignerActionResult CheckConfirmation(JObject data)
		{
			var r = Environment.Context.Tenant.GetService<IDeploymentService>().IsConfirmed(data.Required<Guid>("publisherKey"));

			if (!r)
				return Result.EmptyResult(ViewModel);

			return Result.ViewResult(ViewModel, "~/Views/Ide/Designers/DeploymentLoginForm.cshtml");
		}

		protected string LoginView => "~/Views/Ide/Designers/DeploymentLogin.cshtml";
	}
}
