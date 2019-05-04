using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.ComponentModel;
using TomPIT.Data.DataProviders;
using TomPIT.Deployment;
using TomPIT.Environment;
using TomPIT.Management.Deployment;
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

    public class DeploymentDesigner : DeploymentDesignerBase<MarketplaceElement>
    {
        private List<IPublishedPackage> _publicPackages = null;
        private List<IResourceGroup> _resourceGroups = null;
        private List<IMicroService> _microServices = null;
        private List<IInstallState> _installers = null;
        private Dictionary<Guid, IPackageConfiguration> _configurations = null;
        private List<IPackageDependency> _dependencies = null;
        private List<IPublishedPackage> _dependencyPackages = null;

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
            else if (string.Compare(action, "getChanges", true) == 0)
                return GetChanges(data);
            else if (string.Compare(action, "getCard", true) == 0)
                return GetCard(data);
            else if (string.Compare(action, "delete", true) == 0)
                return Delete(data);

            return base.OnAction(data, action);
        }

        private IDesignerActionResult Delete(JObject data)
        {
            var package = data.Required<Guid>("package");

            Connection.GetService<IDeploymentService>().DeletePackage(package);

            return Result.EmptyResult(ViewModel);
        }

        private IDesignerActionResult GetCard(JObject data)
        {
            var package = data.Required<Guid>("package");
            var pp = Connection.GetService<IDeploymentService>().SelectPublishedPackage(package);

            return Result.ViewResult(new DeploymentPackageCardModel(this, pp), "~/Views/Ide/Designers/DeploymentPackageCard.cshtml");
        }

        private IDesignerActionResult GetChanges(JObject data)
        {
            var timestamp = data.Required<long>("date");

            var r = new JObject
            {
                {"timestamp",DateTime.UtcNow.Ticks }
            };

            var changes = Connection.GetService<IDeploymentService>().QueryInstallAudit(new DateTime(timestamp)).GroupBy(f => f.Package).Select(f => f.First().Package);
            var a = new JArray();
            r.Add("packages", a);

            foreach (var i in changes)
                a.Add(i);

            return Result.JsonResult(ViewModel, r);
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
            var dependency = data.Optional("dependency", Guid.Empty);

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

            Connection.GetService<IDeploymentService>().UpdateInstallerConfiguration(package, config);

            return Result.EmptyResult(ViewModel);
        }

        private IDesignerActionResult InstallConfirm(JObject data)
        {
            var package = data.Required<Guid>("package");

            var r = new List<IInstallState>();

            CreateInstallStack(package, r, Connection.GetService<IDeploymentService>().SelectInstallerConfiguration(package), true);
            Connection.GetService<IDeploymentService>().InsertInstallers(r);

            return Result.EmptyResult(ViewModel);
        }

        private void CreateInstallStack(Guid package, List<IInstallState> items, IPackageConfiguration config, bool isRoot)
        {
            var dependencies = Connection.GetService<IDeploymentService>().QueryDependencies(package);
            IPackageDependency dependency = null;

            foreach (var i in dependencies)
            {
                var d = config.Dependencies.FirstOrDefault(f => f.Dependency == i.Token);

                if (d == null || d.Enabled)
                {
                    CreateInstallStack(i.Token, items, config, false);

                    if (dependency == null)
                        dependency = i;
                }
            }

            if (!isRoot)
            {
                var d = config.Dependencies.FirstOrDefault(f => f.Dependency == package);

                if (d != null && !d.Enabled)
                    return;
            }

            var existing = items.FirstOrDefault(f => f.Package == package);

            if (existing != null)
            {
                if (existing.Parent == Guid.Empty && dependency != null)
                    ((InstallState)existing).Parent = dependency.Token;

                return;
            }

            items.Add(new InstallState
            {
                Package = package,
                Parent = dependency == null ? Guid.Empty : dependency.Token
            });
        }

        private IDesignerActionResult Install(JObject data)
        {
            PackageInfo = Connection.GetService<IDeploymentService>().SelectPublishedPackage(data.Required<Guid>("package"));

            return Result.ViewResult(ViewModel, "~/Views/Ide/Designers/DeploymentPackage.cshtml");
        }

        public IPublishedPackage PackageInfo { get; private set; }
        private List<IMicroService> MicroServices
        {
            get
            {
                if (_microServices == null)
                    _microServices = Connection.GetService<IMicroServiceService>().Query();

                return _microServices;
            }
        }

        private List<IInstallState> Installers
        {
            get
            {
                if (_installers == null)
                    _installers = Connection.GetService<IDeploymentService>().QueryInstallers();

                return _installers;
            }
        }

        public List<IResourceGroup> ResourceGroups
        {
            get
            {
                if (_resourceGroups == null)
                    _resourceGroups = Connection.GetService<IResourceGroupService>().Query();

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

        public PackageState ResolveState(Guid package)
        {
            var state = Installers.FirstOrDefault(f => f.Package == package);
            var ms = MicroServices.FirstOrDefault(f => f.Token == package);

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
                    _dependencies = Connection.GetService<IDeploymentService>().QueryDependencies(PackageInfo.Token);

                return _dependencies;
            }
        }

        public List<IPublishedPackage> DependencyPackages
        {
            get
            {
                if (_dependencyPackages == null)
                    _dependencyPackages = Connection.GetService<IDeploymentService>().QueryPublishedPackage(Dependencies.Select(f=>f.Token).ToList());

                return _dependencyPackages;
            }
        }
    }
}