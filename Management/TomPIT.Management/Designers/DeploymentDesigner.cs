using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Data.DataProviders;
using TomPIT.Deployment;
using TomPIT.Design.Ide.Designers;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Management.Deployment;
using TomPIT.Management.Deployment.Packages;
using TomPIT.Management.Dom;
using TomPIT.Management.Models;
using TomPIT.Security;

namespace TomPIT.Management.Designers
{
	public enum PackageState
	{
		NotInstalled = 1,
		Pending = 2,
		Installing = 3,
		Error = 4,
		Installed = 5,
		PendingUpgrade = 6,
		Upgrading = 7,
		UpgradeAvailable = 8
	}

	public enum MicroserviceListView
	{
		Subscriptions = 1,
		My = 2
	}
	public class DeploymentDesigner : DeploymentDesignerBase<MarketplaceElement>
	{
		private List<IPackageStateDescriptor> _packages = null;
		private List<IResourceGroup> _resourceGroups = null;
		private List<IMicroService> _microServices = null;
		private List<IInstallState> _installers = null;
		private Dictionary<Guid, IPackageConfiguration> _configurations = null;
		private List<IPackageDependency> _dependencies = null;
		private List<IPublishedPackage> _dependencyPackages = null;
		private List<ISubscriptionPlan> _plans = null;
		private List<ISubscription> _subscriptions = null;
		private List<ISubscriptionPlan> _myPlans = null;

		public DeploymentDesigner(MarketplaceElement element) : base(element)
		{
		}

		protected override string MainView => "~/Views/Ide/Designers/Deployment.cshtml";
		public override string View => Account == null ? LoginView : MainView;

		public MicroserviceListView ListView { get; private set; }
		public List<ISubscriptionPlan> Plans
		{
			get
			{
				if (_plans == null)
					_plans = Environment.Context.Tenant.GetService<IDeploymentService>().QuerySubscribedPlans().ToList();

				return _plans;
			}
		}
		private ISubscriptionPlan Plan { get; set; }

		public List<IPackageStateDescriptor> Packages
		{
			get
			{
				if (_packages == null && Plan != null)
				{
					_packages = Environment.Context.Tenant.GetService<IDeploymentService>().QueryPackages(Plan.Token);

					if (_packages != null)
					{
						foreach (var package in _packages)
							package.State = ResolveState(package.Token, package.Service);
					}
				}

				return _packages;
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
			else if (string.Compare(action, "getChanges", true) == 0)
				return GetChanges(data);
			else if (string.Compare(action, "getCard", true) == 0)
				return GetCard(data);
			else if (string.Compare(action, "microserviceList", true) == 0)
				return MicroServiceList(data);

			return base.OnAction(data, action);
		}

		private IDesignerActionResult MicroServiceList(JObject data)
		{
			var plan = data.Required<Guid>("plan");
			ListView = data.Required<MicroserviceListView>("view");

			Plan = Environment.Context.Tenant.GetService<IDeploymentService>().SelectPlan(plan);

			return Result.ViewResult(ViewModel, "~/Views/Ide/Designers/MicroServiceList.cshtml");
		}

		private IDesignerActionResult GetCard(JObject data)
		{
			var microService = data.Required<Guid>("microService");
			var plan = data.Required<Guid>("plan");
			var pp = Environment.Context.Tenant.GetService<IDeploymentService>().SelectPublishedPackage(microService, plan);

			return Result.ViewResult(new DeploymentPackageCardModel(this, pp), "~/Views/Ide/Designers/DeploymentPackageCard.cshtml");
		}

		private IDesignerActionResult GetChanges(JObject data)
		{
			var timestamp = data.Required<long>("date");

			var r = new JObject
				{
					 {"timestamp",DateTime.UtcNow.Ticks }
				};

			var changes = Environment.Context.Tenant.GetService<IDeploymentService>().QueryInstallAudit(new DateTime(timestamp)).GroupBy(f => f.Package).Select(f => f.First().Package);
			var a = new JArray();
			r.Add("packages", a);

			foreach (var i in changes)
				a.Add(i);

			return Result.JsonResult(ViewModel, r);
		}

		private IDesignerActionResult CreateDatabase(JObject data)
		{
			var dp = ResolveDataProvider(data);

			if (dp is IDeployDataProvider deploy)
				deploy.CreateDatabase(data.Required<string>("value"));

			var r = Result.EmptyResult(ViewModel);

			r.MessageKind = InformationKind.Success;
			r.Message = "Database successfully created.";
			r.Title = "Success";

			return r;
		}

		private IDesignerActionResult TestConnection(JObject data)
		{
			var dp = ResolveDataProvider(data);

			dp.TestConnection(Environment.Context, data.Required<string>("value"));

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
			var config = Environment.Context.Tenant.GetService<IDeploymentService>().SelectInstallerConfiguration(package);
			var db = config.Databases.FirstOrDefault(f => string.Compare(database, f.Name, true) == 0);

			if (db == null)
				throw new RuntimeException(SR.ErrDatabaseConfigurationNotFound);

			var r = Environment.Context.Tenant.GetService<IDataProviderService>().Select(db.DataProviderId);

			if (r == null)
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrDataProviderNotFound, db.DataProvider));

			return r;
		}

		private IDesignerActionResult SetOption(JObject data)
		{
			var package = data.Required<Guid>("package");
			var option = data.Required<string>("option");
			var database = data.Optional("database", string.Empty);
			var dependency = data.Optional("dependency", Guid.Empty);

			var config = Environment.Context.Tenant.GetService<IDeploymentService>().SelectInstallerConfiguration(package);

			if (!string.IsNullOrWhiteSpace(database))
			{
				var db = config.Databases.FirstOrDefault(f => string.Compare(database, f.Name, true) == 0);

				if (string.Compare(option, "connectionString", true) == 0)
				{
					var value = data.Optional("value", string.Empty);

					if (!string.IsNullOrWhiteSpace(value))
						value = Environment.Context.Tenant.GetService<ICryptographyService>().Encrypt(value);

					((PackageConfigurationDatabase)db).ConnectionString = value;
				}
				else if (string.Compare(option, "enabled", true) == 0)
					((PackageConfigurationDatabase)db).Enabled = data.Required<bool>("value");
			}
			else if (dependency != Guid.Empty)
			{
				var d = config.Dependencies.FirstOrDefault(f => f.Dependency == dependency);
				var value = data.Optional("value", false);

				if (d == null)
				{
					config.Dependencies.Add(new PackageConfigurationDependency
					{
						Dependency = dependency,
						Enabled = value
					});
				}
				else
					((PackageConfigurationDependency)d).Enabled = value;
			}
			else
			{

				if (string.Compare(option, "runtimeConfiguration", true) == 0)
					((PackageConfiguration)config).RuntimeConfiguration = data.Required<bool>("value");
				else if (string.Compare(option, "resourceGroup", true) == 0)
					((PackageConfiguration)config).ResourceGroup = data.Required<Guid>("value");
			}

			Environment.Context.Tenant.GetService<IDeploymentService>().UpdateInstallerConfiguration(package, config);

			return Result.EmptyResult(ViewModel);
		}

		private IDesignerActionResult InstallConfirm(JObject data)
		{
			var package = data.Required<Guid>("package");

			var r = new List<IInstallState>();
			var pcg = Environment.Context.Tenant.GetService<IDeploymentService>().SelectPublishedPackage(package);
			var dependencyConfiguration = Environment.Context.Tenant.GetService<IDeploymentService>().QueryDependencies(pcg.Service, pcg.Plan);
			var config = Environment.Context.Tenant.GetService<IDeploymentService>().SelectInstallerConfiguration(package);

			var criteria = new List<Tuple<Guid, Guid>>();

			foreach (var dependency in dependencyConfiguration)
				criteria.Add(new Tuple<Guid, Guid>(dependency.MicroService, dependency.Plan));

			var dependencyPackages = criteria.Count > 0 ? Environment.Context.Tenant.GetService<IDeploymentService>().QueryPublishedPackages(criteria) : new List<IPublishedPackage>();

			r.Add(CreateInstallState(pcg.Token, Guid.Empty));

			foreach (var dependency in dependencyConfiguration)
			{
				var targetPackage = dependencyPackages.FirstOrDefault(f => f.Service == dependency.MicroService && f.Plan == dependency.Plan);

				if (targetPackage == null)
					continue;

				var d = config.Dependencies.FirstOrDefault(f => f.Dependency == targetPackage.Token);

				if (d != null && !d.Enabled)
					continue;

				var dependencyPackageConfiguration = Environment.Context.Tenant.GetService<IDeploymentService>().SelectInstallerConfiguration(targetPackage.Token);

				r.Add(CreateInstallState(targetPackage.Token, pcg.Token));
			}

			Environment.Context.Tenant.GetService<IDeploymentService>().InsertInstallers(r);

			return Result.EmptyResult(ViewModel);
		}

		private IInstallState CreateInstallState(Guid package, Guid parent)
		{
			return new InstallState
			{
				Package = package,
				Parent = parent
			};
		}

		private IDesignerActionResult Install(JObject data)
		{
			PackageInfo = Environment.Context.Tenant.GetService<IDeploymentService>().SelectPublishedPackage(data.Required<Guid>("package"));

			return Result.ViewResult(ViewModel, "~/Views/Ide/Designers/DeploymentPackage.cshtml");
		}

		public IPublishedPackage PackageInfo { get; private set; }
		private List<IMicroService> MicroServices
		{
			get
			{
				if (_microServices == null)
					_microServices = Environment.Context.Tenant.GetService<IMicroServiceService>().Query().ToList();

				return _microServices;
			}
		}

		private List<IInstallState> Installers
		{
			get
			{
				if (_installers == null)
					_installers = Environment.Context.Tenant.GetService<IDeploymentService>().QueryInstallers();

				return _installers;
			}
		}

		public List<IResourceGroup> ResourceGroups
		{
			get
			{
				if (_resourceGroups == null)
					_resourceGroups = Environment.Context.Tenant.GetService<IResourceGroupService>().Query().ToList();

				return _resourceGroups;
			}
		}

		public string ErrorMessage(Guid package)
		{
			var d = Installers.FirstOrDefault(f => f.Package == package);

			if (d == null)
				return string.Empty;

			return d.Error;
		}

		public PackageState ResolveState(Guid package, Guid microService)
		{
			var state = Installers.FirstOrDefault(f => f.Package == package);
			var ms = MicroServices.FirstOrDefault(f => f.Token == microService);

			if (state == null && ms == null)
				return PackageState.NotInstalled;

			if (state != null)
			{
				switch (state.Status)
				{
					case InstallStateStatus.Pending:
						if (ms == null)
							return PackageState.Pending;
						else
							return PackageState.PendingUpgrade;
					case InstallStateStatus.Installing:
						if (ms == null)
							return PackageState.Installing;
						else
							return PackageState.Upgrading;
					case InstallStateStatus.Error:
						return PackageState.Error;
					default:
						throw new NotSupportedException();
				}
			}
			else
			{
				if (ms.UpdateStatus == UpdateStatus.UpdateAvailable)
					return PackageState.UpgradeAvailable;
			}

			return PackageState.Installed;
		}

		public Dictionary<Guid, IPackageConfiguration> Configurations
		{
			get
			{
				if (_configurations == null)
					_configurations = new Dictionary<Guid, IPackageConfiguration>();

				return _configurations;
			}
		}

		public List<IPackageDependency> Dependencies
		{
			get
			{
				if (_dependencies == null)
					_dependencies = Environment.Context.Tenant.GetService<IDeploymentService>().QueryDependencies(PackageInfo.Service, PackageInfo.Plan);

				return _dependencies;
			}
		}

		public List<IPublishedPackage> DependencyPackages
		{
			get
			{
				if (_dependencyPackages == null)
				{
					var criteria = new List<Tuple<Guid, Guid>>();

					foreach (var dependency in Dependencies)
						criteria.Add(new Tuple<Guid, Guid>(dependency.MicroService, dependency.Plan));

					_dependencyPackages = Environment.Context.Tenant.GetService<IDeploymentService>().QueryPublishedPackages(criteria);
				}

				return _dependencyPackages;
			}
		}

		public List<ISubscriptionPlan> MyPlans
		{
			get
			{
				if (_myPlans == null)
					_myPlans = Environment.Context.Tenant.GetService<IDeploymentService>().QueryMyPlans().ToList();

				return _myPlans;
			}
		}
		public List<ISubscription> Subscriptions
		{
			get
			{
				if (_subscriptions == null)
					_subscriptions = Environment.Context.Tenant.GetService<IDeploymentService>().QuerySubscriptions().ToList().ToList();

				return _subscriptions;
			}
		}
	}
}