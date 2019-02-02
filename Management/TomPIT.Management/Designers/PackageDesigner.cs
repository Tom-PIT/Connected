using Newtonsoft.Json.Linq;
using System;
using TomPIT.ActionResults;
using TomPIT.ComponentModel;
using TomPIT.Deployment;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Marketplace;

namespace TomPIT.Designers
{
	public class PackageDesigner : DomDesigner<PackageElement>, IMarketplaceProxy, ISignupDesigner
	{
		private JObject _countries = null;
		private IPackage _package = null;
		private IMicroService _microService = null;

		public PackageDesigner(PackageElement element) : base(element)
		{
		}

		public override string View
		{
			get
			{
				var ds = Connection.GetService<IMarketplaceService>();

				if (ds.IsLogged && ds.Publisher != null)
					return "~/Views/Ide/Designers/Package.cshtml";
				else
					return "~/Views/Ide/Designers/PublisherLogin.cshtml";
			}
		}

		public override object ViewModel => this;

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "create", true) == 0)
				return CreatePackage(data);
			else if (string.Compare(action, "signup", true) == 0)
				return Result.ViewResult(this, "~/Views/Ide/Designers/PublisherSignup.cshtml");
			else if (string.Compare(action, "login", true) == 0)
				return Result.ViewResult(this, "~/Views/Ide/Designers/PublisherLoginForm.cshtml");
			else if (string.Compare(action, "signupcreate", true) == 0)
				return SignUp(data);
			else if (string.Compare(action, "checkConfirmation", true) == 0)
				return CheckConfirmation(data);
			else if (string.Compare(action, "authenticate", true) == 0)
				return Login(data);
			else if (string.Compare(action, "logoff", true) == 0)
				return Logoff(data);
			else if (string.Compare(action, "publish", true) == 0)
				return Publish();

			return base.OnAction(data, action);
		}

		private IDesignerActionResult Publish()
		{
			Connection.GetService<IMarketplaceService>().PublishPackage(MicroService.Token);

			var r = Result.EmptyResult(ViewModel);

			r.MessageKind = InformationKind.Success;
			r.Message = "Congratulations! Your package is now online.";
			r.Title = "Package uploaded successfully";

			return r;
		}

		private IDesignerActionResult Logoff(JObject data)
		{
			Connection.GetService<IMarketplaceService>().LogOut();

			return Result.ViewResult(this, "~/Views/Ide/Designers/PublisherLogin.cshtml");
		}

		private IDesignerActionResult Login(JObject data)
		{
			Connection.GetService<IMarketplaceService>().LogIn(data.Required<string>("user"), data.Required<string>("password"), true);

			if (Connection.GetService<IMarketplaceService>().IsLogged)
			{
				var r = Result.ViewResult(ViewModel, "~/Views/Ide/Designers/Package.cshtml");

				r.ResponseHeaders.Add("viewType", "package");

				return r;
			}
			else
			{
				var r = Result.ViewResult(ViewModel, "~/Views/Ide/Designers/PublisherLoginForm.cshtml");

				r.ResponseHeaders.Add("viewType", "login");

				return r;
			}
		}

		private IDesignerActionResult CheckConfirmation(JObject data)
		{
			var r = Connection.GetService<IMarketplaceService>().IsConfirmed(data.Required<Guid>("publisherKey"));

			if (!r)
				return Result.EmptyResult(ViewModel);

			return Result.ViewResult(ViewModel, "~/Views/Ide/Designers/PublisherLoginForm.cshtml");
		}

		private IDesignerActionResult SignUp(JObject data)
		{
			var company = data.Required<string>("company");
			var country = data.Required<int>("country");
			var website = data.Optional("website", string.Empty);
			var firstName = data.Required<string>("firstName");
			var lastName = data.Required<string>("lastName");
			var password = data.Required<string>("password");
			var phone = data.Optional("phone", string.Empty);
			var email = data.Required<string>("email");

			PublisherKey = Connection.GetService<IMarketplaceService>().SignUp(company, firstName, lastName, password, email, country, phone, website);

			return Result.ViewResult(ViewModel, "~/Views/Ide/Designers/PackageInbox.cshtml");
		}

		public IDesignerActionResult CreatePackage(JObject data)
		{
			var name = data.Required<string>("name");
			var title = data.Required<string>("title");
			var version = new Version(data.Required<int>("versionMajor"), data.Required<int>("versionMinor"), data.Required<int>("versionBuild"), data.Required<int>("versionRevision"));
			var scope = data.Required<PackageScope>("scope");
			var trial = data.Required<bool>("trial");
			var trialPeriod = data.Required<int>("trialPeriod");
			var description = data.Optional("description", string.Empty);
			var price = data.Required<double>("price");
			var tags = data.Optional("tags", string.Empty);
			var projectUrl = data.Optional("projectUrl", string.Empty);
			var licenseUrl = data.Optional("licenseUrl", string.Empty);
			var imageUrl = data.Optional("imageUrl", string.Empty);
			var licenses = data.Optional("licenses", string.Empty);

			Connection.GetService<IMarketplaceService>().CreatePackage(MicroService.Token, name, title, version.ToString(), scope, trial, trialPeriod, description, price,
				tags, projectUrl, imageUrl, licenseUrl, licenses, null);

			var r = Result.EmptyResult(ViewModel);

			r.MessageKind = InformationKind.Success;
			r.Message = "You can now upload package to the marketplace.";
			r.Title = "Package created successfully";

			return r;
		}

		public IPublisher Publisher { get { return Connection.GetService<IMarketplaceService>().Publisher; } }
		public JObject Countries
		{
			get
			{
				if (_countries == null) { _countries = Connection.GetService<IMarketplaceService>().QueryCountries(); }

				return _countries;
			}
		}

		public Guid PublisherKey { get; private set; }

		public IPackage Package
		{
			get
			{
				if (_package == null)
					_package = Connection.GetService<IMarketplaceService>().SelectPackage(MicroService.Token);

				return _package;
			}
		}

		public IMicroService MicroService
		{
			get
			{
				if (_microService == null)
					_microService = DomQuery.Closest<IMicroServiceScope>(Element).MicroService;

				return _microService;
			}
		}

		public JArray Tags => new JArray();
	}
}
