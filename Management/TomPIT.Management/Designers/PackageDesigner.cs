using Newtonsoft.Json.Linq;
using TomPIT.ActionResults;
using TomPIT.Deployment;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Marketplace;

namespace TomPIT.Designers
{
	public class PackageDesigner : DomDesigner<PackageElement>, IMarketplaceProxy
	{
		private JObject _countries = null;

		public PackageDesigner(IEnvironment environment, PackageElement element) : base(environment, element)
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
				return CreatePackage();
			else if (string.Compare(action, "signup", true) == 0)
				return Result.ViewResult(this, "~/Views/Ide/Designers/PublisherSignup.cshtml");
			else if (string.Compare(action, "login", true) == 0)
				return Result.ViewResult(this, "~/Views/Ide/Designers/PublisherLoginForm.cshtml");
			else if (string.Compare(action, "signupcreate", true) == 0)
				return SignUp(data);

			return base.OnAction(data, action);
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

			Connection.GetService<IMarketplaceService>().SignUp(company, firstName, lastName, password, email, country, phone, website);


			return Result.ViewResult(ViewModel, "~/Views/Ide/Designers/PackageInbox.cshtml");
		}

		public IDesignerActionResult CreatePackage()
		{
			var r = Package.Create(new PackageCreateArgs(Environment.Context.Connection(), DomQuery.Closest<IMicroServiceScope>(Element).MicroService.Token, new PackageMetaData
			{
				Price = 399
			}, (f) =>
			{

			}));

			return Result.EmptyResult(ViewModel);
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
	}
}
