using Newtonsoft.Json.Linq;
using System;
using TomPIT.ActionResults;
using TomPIT.ComponentModel;
using TomPIT.Deployment;
using TomPIT.Dom;
using TomPIT.Management.Deployment;
using TomPIT.Management.Designers;

namespace TomPIT.Designers
{
	public class PackageDesigner : DeploymentDesignerBase<PackageElement>, ISignupDesigner
	{
		private IPackage _package = null;
		private IMicroService _microService = null;

		public PackageDesigner(PackageElement element) : base(element)
		{
		}

		protected override string MainView => "~/Views/Ide/Designers/Package.cshtml";
		public override string View => Account == null ? LoginView : MainView;

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "create", true) == 0)
				return CreatePackage(data);
			else if (string.Compare(action, "publish", true) == 0)
				return Publish();

			return base.OnAction(data, action);
		}

		private IDesignerActionResult Publish()
		{
			Connection.GetService<IDeploymentService>().PublishPackage(MicroService.Token);

			var r = Result.EmptyResult(ViewModel);

			r.MessageKind = InformationKind.Success;
			r.Message = "Congratulations! Your package is now online.";
			r.Title = "Package uploaded successfully";

			return r;
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
			var rt = data.Optional("runtimeConfigurationSupported", false);
			var av = data.Optional("autoVersion", true);

			Connection.GetService<IDeploymentService>().CreatePackage(MicroService.Token, name, title, version.ToString(), scope, trial, trialPeriod, description, price,
				tags, projectUrl, imageUrl, licenseUrl, licenses, rt, av);

			var r = Result.EmptyResult(ViewModel);

			r.MessageKind = InformationKind.Success;
			r.Message = "You can now upload package to the marketplace.";
			r.Title = "Package created successfully";

			return r;
		}


		public IPackage Package
		{
			get
			{
				if (_package == null)
					_package = Connection.GetService<IDeploymentService>().SelectPackage(MicroService.Token);

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

		public Version Version
		{
			get
			{
				if (Package == null)
					return IncrementVersion(new Version(0, 0, 0, 0));
				else
				{
					if (Package.Configuration.AutoVersioning)
						return IncrementVersion(Version.Parse(Package.MetaData.Version));

					return Version.Parse(Package.MetaData.Version);
				}
			}
		}

		private Version IncrementVersion(Version existing)
		{
			var build = Convert.ToInt32(string.Format("{0}{1}", DateTime.Today.Month.ToString(), DateTime.Today.Day.ToString("00")));
			var revision = 0;

			if (existing.Build == build)
				revision = existing.Revision + 1;

			return new Version(existing.Major, existing.Minor, build, revision);
		}
	}
}
