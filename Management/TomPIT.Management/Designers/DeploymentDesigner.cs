using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.Data.DataProviders;
using TomPIT.Deployment;
using TomPIT.Environment;
using TomPIT.Management.Deployment;
using TomPIT.Management.Dom;
using TomPIT.Security;

namespace TomPIT.Management.Designers
{
	public class DeploymentDesigner : DeploymentDesignerBase<MarketplaceElement>
	{
		private List<IPublishedPackage> _publicPackages = null;
		private List<IResourceGroup> _resourceGroups = null;

		public DeploymentDesigner(MarketplaceElement element) : base(element)
		{
		}

		protected override string MainView => "~/Views/Ide/Designers/Deployment.cshtml";
		public override string View => MainView;

		public List<IPublishedPackage> PublicPackages
		{
			get
			{
				if (_publicPackages == null)
					_publicPackages = Connection.GetService<IDeploymentService>().QueryPublicPackages();

				return _publicPackages;
			}
		}

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "install", true) == 0)
				return Install(data);
			else if (string.Compare(action, "installConfirm", true) == 0)
				return InstallConfirm(data);
			else if (string.Compare(action, "setOption", true) == 0)
				return SetOption(data);
			else if (string.Compare(action, "testConnection", true) == 0)
				return TestConnection(data);
			else if (string.Compare(action, "createDatabase", true) == 0)
				return CreateDatabase(data);

			return base.OnAction(data, action);
		}

		private IDesignerActionResult CreateDatabase(JObject data)
		{
			var dp = ResolveDataProvider(data);

			dp.CreateDatabase(data.Required<string>("value"));

			var r = Result.EmptyResult(ViewModel);

			r.MessageKind = InformationKind.Success;
			r.Message = "Database successfully created.";
			r.Title = "Success";

			return r;
		}

		private IDesignerActionResult TestConnection(JObject data)
		{
			var dp = ResolveDataProvider(data);

			dp.TestConnection(data.Required<string>("value"));

			var r = Result.EmptyResult(ViewModel);

			r.MessageKind = InformationKind.Success;
			r.Message = "Connection string is valid and system can connect to this database";
			r.Title = "Success";

			return r;
		}

		private IDataProvider ResolveDataProvider(JObject data)
		{
			var package = data.Required<Guid>("package");
			var value = data.Required<string>("value");
			var database = data.Required<string>("database");
			var config = Connection.GetService<IDeploymentService>().SelectInstallerConfiguration(package);
			var db = config.Databases.FirstOrDefault(f => string.Compare(database, f.Name, true) == 0);

			if (db == null)
				throw new RuntimeException(SR.ErrDatabaseConfigurationNotFound);

			var r = Connection.GetService<IDataProviderService>().Select(db.DataProviderId);

			if (r == null)
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrDataProviderNotFound, db.DataProvider));

			return r;
		}

		private IDesignerActionResult SetOption(JObject data)
		{
			var package = data.Required<Guid>("package");
			var option = data.Required<string>("option");
			var database = data.Optional("database", string.Empty);

			var config = Connection.GetService<IDeploymentService>().SelectInstallerConfiguration(package);

			if (!string.IsNullOrWhiteSpace(database))
			{
				var db = config.Databases.FirstOrDefault(f => string.Compare(database, f.Name, true) == 0);

				if (string.Compare(option, "connectionString", true) == 0)
				{
					var value = data.Optional("value", string.Empty);

					if (!string.IsNullOrWhiteSpace(value))
						value = Connection.GetService<ICryptographyService>().Encrypt(value);

					((PackageConfigurationDatabase)db).ConnectionString = value;
				}
				else if (string.Compare(option, "enabled", true) == 0)
					((PackageConfigurationDatabase)db).Enabled = data.Required<bool>("value");
			}
			else
			{

				if (string.Compare(option, "runtimeConfiguration", true) == 0)
					((PackageConfiguration)config).RuntimeConfiguration = data.Required<bool>("value");
				else if (string.Compare(option, "resourceGroup", true) == 0)
					((PackageConfiguration)config).ResourceGroup = data.Required<Guid>("value");
			}

			Connection.GetService<IDeploymentService>().UpdateInstallerConfiguration(package, config);

			return Result.EmptyResult(ViewModel);
		}

		private IDesignerActionResult InstallConfirm(JObject data)
		{
			var packages = data.Required<JArray>("packages");
			var items = new List<IInstallState>();

			foreach (JValue i in packages)
			{
				items.Add(new InstallState
				{
					Package = Types.Convert<Guid>(i.Value<string>())
				});
			}

			Connection.GetService<IDeploymentService>().InsertInstallers(items);

			return Result.EmptyResult(ViewModel);
		}

		private IDesignerActionResult Install(JObject data)
		{
			PackageInfo = Connection.GetService<IDeploymentService>().SelectPublishedPackage(data.Required<Guid>("package"));

			return Result.ViewResult(ViewModel, "~/Views/Ide/Designers/DeploymentPackage.cshtml");
		}

		public IPublishedPackage PackageInfo { get; private set; }

		public List<IResourceGroup> ResourceGroups
		{
			get
			{
				if (_resourceGroups == null)
					_resourceGroups = Connection.GetService<IResourceGroupService>().Query();

				return _resourceGroups;
			}
		}
	}
}
