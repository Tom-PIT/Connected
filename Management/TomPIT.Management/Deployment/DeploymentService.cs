using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Design.Serialization;
using TomPIT.Environment;
using TomPIT.Storage;

namespace TomPIT.Management.Deployment
{
	internal class DeploymentService : IDeploymentService
	{
		private bool? _isLogged = null;
		private IAccount _account = null;

		public DeploymentService(ISysConnection connection)
		{
			Connection = connection;
		}

		public bool IsLogged
		{
			get
			{
				if (_isLogged == null)
				{
					var u = Connection.CreateUrl("DeploymentManagement", "IsLogged");

					_isLogged = Connection.Get<bool>(u);
				}

				return (bool)_isLogged;
			}
		}

		public IAccount Account
		{
			get
			{
				if (_account == null)
				{
					var u = Connection.CreateUrl("DeploymentManagement", "SelectAccount");

					_account = Connection.Get<Account>(u);
				}

				return _account;
			}
		}

		private ISysConnection Connection { get; }

		public void LogOut()
		{
			var u = Connection.CreateUrl("DeploymentManagement", "Logout");

			Connection.Post(u);

			_isLogged = null;
			_account = null;
		}

		public void LogIn(string userName, string password)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "Login");
			var e = new JObject
				{
					 {"user", userName },
					 {"password", password }
				};

			_isLogged = null;
			_account = null;

			Connection.Post(u, e);

			if (!IsLogged)
				throw new Exception(SR.ErrLoginFailed);
		}

		public Guid SignUp(string company, string firstName, string lastName, string password, string email, string country, string phone, string website)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "Signup");
			var e = new JObject
				{
					 {"company", company },
					 {"firstName", firstName },
					 {"lastName", lastName },
					 {"password", password },
					 {"email", email },
					 {"country", country },
					 {"phone", phone },
					 {"website", website }
				};

			return Connection.Post<Guid>(u, e);
		}

		public List<ICountry> QueryCountries()
		{
			var url = Connection.CreateUrl("DeploymentManagement", "QueryCountries");

			return Connection.Get<List<Country>>(url).ToList<ICountry>();
		}

		public bool IsConfirmed(Guid accountKey)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "IsConfirmed");
			var e = new JObject
				{
					 {"accountKey", accountKey }
				};

			return Connection.Post<bool>(u, e);
		}

		public void CreatePackage(Guid microService, Guid plan, string name, string title, string version, string description, string tags, string projectUrl, string imageUrl, string licenseUrl, string licenses, bool runtimeConfigurationSupported,
			 bool autoVersion)
		{
			var ms = Connection.GetService<IMicroServiceService>().Select(microService);
			
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

			package.Create(microService, Connection);

			var blob = new Blob
			{
				ContentType = "application/json",
				FileName = string.Format("{0}.json", ms.Name),
				MicroService = microService,
				PrimaryKey = microService.ToString(),
				Type = BlobTypes.Package
			};

			var id = Connection.GetService<IStorageService>().Upload(blob, Connection.GetService<ISerializationService>().Serialize(package), StoragePolicy.Singleton);

			if (ms.Package != id || ms.Plan != plan)
				Connection.GetService<IMicroServiceManagementService>().Update(microService, ms.Name, ms.Status, ms.Template, ms.ResourceGroup, id, plan, ms.UpdateStatus, ms.CommitStatus);
		}

		public IPackage SelectPackage(Guid microService)
		{
			var ms = Connection.GetService<IMicroServiceService>().Select(microService);

			if (ms == null || ms.Package == Guid.Empty)
				return null;

			var content = Connection.GetService<IStorageService>().Download(ms.Package);

			if (content == null || content.Content == null || content.Content.Length == 0)
				return null;

			try
			{
				return (Package)Connection.GetService<ISerializationService>().Deserialize(content.Content, typeof(Package));
			}
			catch (Exception ex)
			{
				Connection.LogError(null, nameof(DeploymentService), ex.Source, ex.Message);
				return null;
			}
		}

		public IPackage DownloadPackage(Guid package)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "DownloadPackage");
			var e = new JObject
				{
					 {"package", package}
				};

			var raw = Connection.Post<byte[]>(u, e);

			if (raw == null || raw.Length == 0)
				return null;

			return (Package)Connection.GetService<ISerializationService>().Deserialize(raw, typeof(Package));
		}

		public void PublishPackage(Guid microService)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "PublishPackage");
			var e = new JObject
				{
					 {"microService", microService}
				};

			Connection.Post(u, e);

			var ms = Connection.GetService<IMicroServiceService>().Select(microService);

			if (ms != null)
				Connection.GetService<IMicroServiceManagementService>().Update(ms.Token, ms.Name, ms.Status, ms.Template, ms.ResourceGroup, ms.Package, ms.Plan, ms.UpdateStatus, CommitStatus.Synchronized);
		}

		public List<IPackageStateDescriptor> QueryPackages(Guid plan)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "QueryPackages");
			var e = new JObject
			{
				{"plan", plan }
			};

			return Connection.Post<List<PublishedPackage>>(u, e).ToList<IPackageStateDescriptor>();
		}

		public void InsertInstallers(List<IInstallState> installers)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "InsertInstallers");
			var e = new JObject();
			var a = new JArray();

			e.Add("installers", a);

			foreach (var i in installers)
			{
				var o = new JObject
					 {
						  { "package", i.Package }
					 };

				if (i.Parent != Guid.Empty)
					o.Add("parent", i.Parent);

				a.Add(o);
			}

			Connection.Post(u, e);
		}

		public List<IInstallState> QueryInstallers()
		{
			var u = Connection.CreateUrl("DeploymentManagement", "QueryInstallers");

			return Connection.Get<List<InstallState>>(u).ToList<IInstallState>();
		}

		public void UpdateInstaller(Guid package, InstallStateStatus status, string error)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "UpdateInstaller");
			var e = new JObject
				{
					 {"package", package },
					 {"status", status.ToString() },
					 {"error", error }
				};

			Connection.Post(u, e);
		}

		public void DeleteInstaller(Guid package)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "DeleteInstaller");
			var e = new JObject
				{
					 {"package", package }
				};

			Connection.Post(u, e);
		}

		public void Deploy(Guid id, IPackage package)
		{
			new PackageDeployment(Connection, id, package).Deploy();
		}

		public List<IPublishedPackage> QueryPublishedPackages(List<Tuple<Guid, Guid>> packages)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "QueryPublishedPackages");
			var e = new JObject();
			var a = new JArray();

			e.Add("packages", a);

			foreach (var package in packages)
			{
				a.Add(new JObject
				{
					{"microService", package.Item1 },
					{"plan", package.Item2 }
				});
			}

			return Connection.Post<List<PublishedPackage>>(u, e).ToList<IPublishedPackage>();
		}

		public IPublishedPackage SelectPublishedPackage(Guid microService, Guid plan)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "SelectPublishedPackage");
			var e = new JObject
				{
					 {"microService", microService },
					 {"plan", plan }
				};

			return Connection.Post<PublishedPackage>(u, e);
		}

		public IPackageConfiguration SelectInstallerConfiguration(Guid package)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "DownloadConfiguration");
			var e = new JObject
				{
					 {"package", package}
				};

			var raw = Connection.Post<byte[]>(u, e);

			if (raw == null || raw.Length == 0)
				return null;

			var config = (PackageConfiguration)Connection.GetService<ISerializationService>().Deserialize(raw, typeof(PackageConfiguration));

			var ms = Connection.GetService<IMicroServiceService>().Select(package);
			PackageConfiguration existing = SelectExistingInstallerConfiguration(package);

			SynchronizeConfiguration(config, existing);

			return config;
		}

		public void UpdateInstallerConfiguration(Guid package, IPackageConfiguration configuration)
		{
			var id = SelectInstallerConfigurationId(package);

			var blobId = Connection.GetService<IStorageService>().Upload(new Blob
			{
				ContentType = "application/json",
				FileName = string.Format("{0}.json", package),
				MicroService = Guid.Empty,
				PrimaryKey = package.ToString(),
				Type = BlobTypes.InstallerConfiguration,
				ResourceGroup = Connection.GetService<IResourceGroupService>().Default.Token,
			}, Connection.GetService<ISerializationService>().Serialize(configuration), StoragePolicy.Singleton);

			if (id != blobId)
			{
				var u = Connection.CreateUrl("DeploymentManagement", "InsertInstallerConfiguration");
				var e = new JObject
					 {
						  {"package", package },
						  {"configuration", blobId }
					 };

				Connection.Post(u, e);
			}
		}

		private void SynchronizeConfiguration(PackageConfiguration configuration, PackageConfiguration existing)
		{
			var resourceGroups = Connection.GetService<IResourceGroupService>().Query();
			var defaultResourceGroup = resourceGroups[0].Token;

			configuration.ResourceGroup = defaultResourceGroup;

			if (existing == null)
				return;

			configuration.ResourceGroup = existing.ResourceGroup;
			configuration.RuntimeConfiguration = existing.RuntimeConfiguration;

			foreach (var i in configuration.Databases)
			{
				var ed = existing.Databases.FirstOrDefault(f => f.Connection == i.Connection);

				if (ed == null)
					continue;

				var db = i as PackageConfigurationDatabase;

				db.ConnectionString = ed.ConnectionString;
				db.DataProvider = ed.DataProvider;
				db.DataProviderId = ed.DataProviderId;
				db.Enabled = ed.Enabled;
				db.Name = ed.Name;
			}

			if (existing != null)
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
			var u = Connection.CreateUrl("DeploymentManagement", "SelectInstallerConfiguration");
			var e = new JObject
				{
					 {"package", package }
				};

			return Connection.Post<Guid>(u, e);
		}

		private PackageConfiguration SelectExistingInstallerConfiguration(Guid package)
		{
			var id = SelectInstallerConfigurationId(package);

			if (id == Guid.Empty)
				return null;

			var content = Connection.GetService<IStorageService>().Download(id);

			if (content == null || content.Content.Length == 0)
				return null;

			return (PackageConfiguration)Connection.GetService<ISerializationService>().Deserialize(content.Content, typeof(PackageConfiguration));
		}

		public List<IPackageDependency> QueryDependencies(Guid microService, Guid plan)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "QueryDependencies");
			var e = new JObject
				{
					{"microService", microService },
					{"plan", plan }
				};

			return Connection.Post<List<PackageDependency>>(u, e).ToList<IPackageDependency>();
		}

		public List<IInstallAudit> QueryInstallAudit(DateTime from)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "QueryInstallAudit");
			var e = new JObject
				{
					 {"from", from }
				};

			return Connection.Post<List<InstallAudit>>(u, e).ToList<IInstallAudit>();
		}

		public List<IInstallAudit> QueryInstallAudit(Guid package)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "QueryInstallAudit");
			var e = new JObject
				{
					 {"package", package }
				};

			return Connection.Post<List<InstallAudit>>(u, e).ToList<IInstallAudit>();
		}

		public List<IPackageVersion> CheckForUpdates(List<IMicroService> microServices)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "CheckForUpdates");
			var e = new JObject();
			var a = new JArray();

			e.Add("packages", a);

			foreach (var microService in microServices)
			{
				var v = Version.Parse(microService.Version);

				a.Add(new JObject
					 {
						  {"microService", microService.Token },
						  {"plan", microService.Plan },
						  {"major", v.Major },
						  {"minor", v.Minor },
						  {"build", v.Build },
						  {"revision", v.Revision }
					 });
			}

			return Connection.Post<List<PackageVersion>>(u, e).ToList<IPackageVersion>();
		}

		public ISubscriptionPlan SelectPlan(Guid token)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "SelectPlan");
			var e = new JObject
			{
				{"token", token }
			};

			return Connection.Post<SubscriptionPlan>(u, e);
		}

		public List<ISubscriptionPlan> QuerySubscribedPlans()
		{
			var u = Connection.CreateUrl("DeploymentManagement", "QuerySubscribedPlans");

			return Connection.Get<List<SubscriptionPlan>>(u).ToList<ISubscriptionPlan>();
		}

		public List<ISubscriptionPlan> QueryMyPlans()
		{
			var u = Connection.CreateUrl("DeploymentManagement", "QueryMyPlans");

			return Connection.Get<List<SubscriptionPlan>>(u).ToList<ISubscriptionPlan>();
		}

		public IPublishedPackage SelectPublishedPackage(Guid token)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "SelectPublishedPackageByToken");
			var e = new JObject
			{
					{"token", token }
			};

			return Connection.Post<PublishedPackage>(u, e);
		}

		public List<string> QueryTags()
		{
			var u = Connection.CreateUrl("DeploymentManagement", "QueryTags");

			return Connection.Get<List<string>>(u);
		}

		public List<ISubscription> QuerySubscriptions()
		{
			var u = Connection.CreateUrl("DeploymentManagement", "QuerySubscriptions");

			return Connection.Get<List<Subscription>>(u).ToList<ISubscription>();
		}
	}
}
