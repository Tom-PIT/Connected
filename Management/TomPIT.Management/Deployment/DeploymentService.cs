using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Design.Serialization;
using TomPIT.Diagnostics;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Management.ComponentModel;
using TomPIT.Management.Deployment.Packages;
using TomPIT.Middleware;
using TomPIT.Storage;

namespace TomPIT.Management.Deployment
{
    internal class DeploymentService : TenantObject, IDeploymentService
    {
        private bool? _isLogged = null;
        private IAccount _account = null;

        public DeploymentService(ITenant tenant) : base(tenant)
        {
        }

        public bool IsLogged => _isLogged ??= Instance.SysProxy.Management.Deployment.SelectIsLogged();

        public IAccount Account => _account ??= Instance.SysProxy.Management.Deployment.SelectAccount();

        public void LogOut()
        {
            Instance.SysProxy.Management.Deployment.LogOut();

            _isLogged = null;
            _account = null;
        }

        public void LogIn(string userName, string password)
        {
            Instance.SysProxy.Management.Deployment.LogIn(userName, password);

            _isLogged = null;
            _account = null;

            if (!IsLogged)
                throw new Exception(SR.ErrLoginFailed);
        }

        public Guid SignUp(string company, string firstName, string lastName, string password, string email, string country, string phone, string website)
        {
            return Instance.SysProxy.Management.Deployment.SignUp(company, firstName, lastName, password, email, country, phone, website);
        }

        public List<ICountry> QueryCountries()
        {
            return Instance.SysProxy.Management.Deployment.QueryCountries().ToList();
        }

        public bool IsConfirmed(Guid accountKey)
        {
            return Instance.SysProxy.Management.Deployment.IsConfirmed(accountKey);
        }

        public void CreatePackage(Guid microService, Guid plan, string name, string title, string version, string description, string tags, string projectUrl, string imageUrl, string licenseUrl, string licenses, bool runtimeConfigurationSupported,
             bool autoVersion)
        {
            var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

            var package = new Package();
            var m = package.MetaData as PackageMetaData;

            m.Description = description;
            m.ImageUrl = imageUrl;
            m.LicenseUrl = licenseUrl;
            m.Name = name;
            m.ProjectUrl = projectUrl;
            m.Tags = tags;
            m.Title = title;
            m.Version = version;
            m.Licenses = licenses;
            m.Service = microService;
            m.Account = Account.Key;
            m.Created = DateTime.UtcNow;
            m.Plan = plan;
            m.ShellVersion = Shell.Version.ToString();

            ((PackageConfiguration)package.Configuration).RuntimeConfigurationSupported = runtimeConfigurationSupported;
            ((PackageConfiguration)package.Configuration).AutoVersioning = autoVersion;

            package.Create(microService, Tenant);

            var blob = new Blob
            {
                ContentType = "application/json",
                FileName = string.Format("{0}.json", ms.Name),
                MicroService = microService,
                PrimaryKey = microService.ToString(),
                Type = BlobTypes.Package
            };

            var id = Tenant.GetService<IStorageService>().Upload(blob, Tenant.GetService<ISerializationService>().Serialize(package), StoragePolicy.Singleton);

            if (ms.Package != id || ms.Plan != plan)
                Tenant.GetService<IMicroServiceManagementService>().Update(microService, ms.Name, ms.Status, ms.Template, ms.ResourceGroup, id, plan, ms.UpdateStatus, ms.CommitStatus);
        }

        public IPackage SelectPackage(Guid microService)
        {
            var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

            if (ms == null || ms.Package == Guid.Empty)
                return null;

            var content = Tenant.GetService<IStorageService>().Download(ms.Package);

            if (content == null || content.Content == null || content.Content.Length == 0)
                return null;

            try
            {
                return (Package)Tenant.GetService<ISerializationService>().Deserialize(content.Content, typeof(Package));
            }
            catch (Exception ex)
            {
                Tenant.LogError(ex.Source, ex.Message, nameof(DeploymentService));
                return null;
            }
        }

        public IPackage DownloadPackage(Guid package)
        {
            var raw = Instance.SysProxy.Management.Deployment.DownloadPackage(package);

            if (raw is null || !raw.Any())
                return null;

            return (Package)Tenant.GetService<ISerializationService>().Deserialize(raw, typeof(Package));
        }

        public void PublishPackage(Guid microService)
        {
            Instance.SysProxy.Management.Deployment.PublishPackage(microService);

            var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

            if (ms is not null)
                Tenant.GetService<IMicroServiceManagementService>().Update(ms.Token, ms.Name, ms.Status, ms.Template, ms.ResourceGroup, ms.Package, ms.Plan, ms.UpdateStatus, CommitStatus.Synchronized);
        }

        public List<IPublishedPackage> QueryPackages(Guid plan)
        {
            return Instance.SysProxy.Management.Deployment.QueryPackages(plan).ToList();
        }

        public void InsertInstallers(List<IInstallState> installers)
        {
            Instance.SysProxy.Management.Deployment.InsertInstallers(installers);
        }

        public List<IInstallState> QueryInstallers()
        {
            return Instance.SysProxy.Management.Deployment.QueryInstallers().ToList();
        }

        public void UpdateInstaller(Guid package, InstallStateStatus status, string error)
        {
            Instance.SysProxy.Management.Deployment.UpdateInstaller(package, status, error);
        }

        public void DeleteInstaller(Guid package)
        {
            Instance.SysProxy.Management.Deployment.DeleteInstaller(package);
        }

        public void Deploy(Guid id, IPackage package)
        {
            new PackageDeployment(Tenant, id, package).Deploy();
        }

        public List<IPublishedPackage> QueryPublishedPackages(List<Tuple<Guid, Guid>> packages)
        {
            return Instance.SysProxy.Management.Deployment.QueryPublishedPackages(packages).ToList();
        }

        public IPublishedPackage SelectPublishedPackage(Guid microService, Guid plan)
        {
            return Instance.SysProxy.Management.Deployment.SelectPublishedPackage(microService, plan);
        }

        public IPackageConfiguration SelectInstallerConfiguration(Guid package)
        {
            var raw = Instance.SysProxy.Management.Deployment.DownloadInstallerConfiguration(package);

            if (raw is null || !raw.Any())
                return null;

            var config = (PackageConfiguration)Tenant.GetService<ISerializationService>().Deserialize(raw, typeof(PackageConfiguration));
            var existing = SelectExistingInstallerConfiguration(package);

            SynchronizeConfiguration(config, existing);

            return config;
        }

        public void UpdateInstallerConfiguration(Guid package, IPackageConfiguration configuration)
        {
            var id = SelectInstallerConfigurationId(package);

            var blobId = Tenant.GetService<IStorageService>().Upload(new Blob
            {
                ContentType = "application/json",
                FileName = $"{package}.json",
                MicroService = Guid.Empty,
                PrimaryKey = package.ToString(),
                Type = BlobTypes.InstallerConfiguration,
                ResourceGroup = Tenant.GetService<IResourceGroupService>().Default.Token,
            }, Tenant.GetService<ISerializationService>().Serialize(configuration), StoragePolicy.Singleton);

            if (id != blobId)
                Instance.SysProxy.Management.Deployment.InsertInstallerConfiguration(package, blobId);
        }

        private void SynchronizeConfiguration(PackageConfiguration configuration, PackageConfiguration existing)
        {
            var resourceGroups = Tenant.GetService<IResourceGroupService>().Query();
            var defaultResourceGroup = resourceGroups[0].Token;

            configuration.ResourceGroup = defaultResourceGroup;

            if (existing is null)
                return;

            configuration.ResourceGroup = existing.ResourceGroup;
            configuration.RuntimeConfiguration = existing.RuntimeConfiguration;

            foreach (var i in configuration.Databases)
            {
                var ed = existing.Databases.FirstOrDefault(f => f.Connection == i.Connection);

                if (ed is null)
                    continue;

                var db = i as PackageConfigurationDatabase;

                db.ConnectionString = ed.ConnectionString;
                db.DataProvider = ed.DataProvider;
                db.DataProviderId = ed.DataProviderId;
                db.Enabled = ed.Enabled;
                db.Name = ed.Name;
            }

            if (existing is not null)
            {
                foreach (var i in existing.Dependencies)
                {
                    configuration.Dependencies.Add(new PackageConfigurationDependency
                    {
                        Dependency = i.Dependency,
                        Enabled = i.Enabled
                    });
                }
            }
        }

        private Guid SelectInstallerConfigurationId(Guid package)
        {
            return Instance.SysProxy.Management.Deployment.SelectInstallerConfigurationId(package);
        }

        private PackageConfiguration SelectExistingInstallerConfiguration(Guid package)
        {
            var id = SelectInstallerConfigurationId(package);

            if (id == Guid.Empty)
                return default;

            var content = Tenant.GetService<IStorageService>().Download(id);

            if (content == null || content.Content.Length == 0)
                return default;

            try
            {
                /*
				 * this exception is thrown when migrating from version 1.1.905 to newer because
				 * there were breaking changes in the configuration schema
				 */
                return (PackageConfiguration)Tenant.GetService<ISerializationService>().Deserialize(content.Content, typeof(PackageConfiguration));
            }
            catch (RuntimeException)
            {
                return default;
            }
        }

        public List<IPackageDependency> QueryDependencies(Guid microService, Guid plan)
        {
            return Instance.SysProxy.Management.Deployment.QueryDependencies(microService, plan).ToList();
        }

        public List<IInstallAudit> QueryInstallAudit(DateTime from)
        {
            return Instance.SysProxy.Management.Deployment.QueryInstallAudit(from).ToList();
        }

        public List<IInstallAudit> QueryInstallAudit(Guid package)
        {
            return Instance.SysProxy.Management.Deployment.QueryInstallAudit(package).ToList();
        }

        public List<IPackageVersion> CheckForUpdates(List<IMicroService> microServices)
        {
            var packages = new List<IPackageVersion>();

            foreach (var microService in microServices)
            {
                var v = Version.Parse(microService.Version);

                packages.Add(new PackageVersion
                {
                    MicroService = microService.Token,
                    Plan = microService.Plan,
                    Major = v.Major,
                    Minor = v.Minor,
                    Build = v.Build,
                    Revision = v.Revision
                });
            }

            return Instance.SysProxy.Management.Deployment.CheckForUpdates(packages).ToList();
        }

        public ISubscriptionPlan SelectPlan(Guid token)
        {
            return Instance.SysProxy.Management.Deployment.SelectPlan(token);
        }

        public List<ISubscriptionPlan> QuerySubscribedPlans()
        {
            return Instance.SysProxy.Management.Deployment.QuerySubscribedPlans().ToList();
        }

        public List<ISubscriptionPlan> QueryMyPlans()
        {
            return Instance.SysProxy.Management.Deployment.QueryMyPlans().ToList();
        }

        public IPublishedPackage SelectPublishedPackage(Guid token)
        {
            return Instance.SysProxy.Management.Deployment.SelectPublishedPackage(token);
        }

        public List<string> QueryTags()
        {
            return Instance.SysProxy.Management.Deployment.QueryTags().ToList();
        }

        public List<ISubscription> QuerySubscriptions()
        {
            return Instance.SysProxy.Management.Deployment.QuerySubscriptions().ToList();
        }
    }
}
