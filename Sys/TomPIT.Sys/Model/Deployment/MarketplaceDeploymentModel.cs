using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Security;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Model.Deployment
{
	public class MarketplaceDeploymentModel
	{

		public bool IsLogged { get; private set; }
		public IAccount Account { get; private set; }
		private Guid AuthenticationToken { get; set; }
		private string UserName { get; set; }
		private string Password { get; set; }

		public MarketplaceDeploymentModel()
		{
			TryLogin();
		}

		public void Login(string user, string password)
		{
			var url = new MarketplaceUrl("IIdentities", "Authenticate");

			var body = new JObject
				{
					 {"user", user},
					 {"password", password }
				};

			var ar = new HttpConnection().Post<Guid>(url, body);

			if (ar != Guid.Empty)
			{
				IsLogged = true;
				AuthenticationToken = ar;
				UserName = user;
				Password = password;

				var setting = DataModel.Settings.Select("marketplaceCredentials", null, null, null);
				var value = Shell.GetService<ISysCryptographyService>().Encrypt(this, string.Format("{0} {1}", user, password));
				DataModel.Settings.Update("marketplaceCredentials", null, null, null, value);

				LoadAccountProfile();
			}
			else
				throw new Exception(SR.ErrLoginFailed);
		}

		private void LoadAccountProfile()
		{
			var url = new MarketplaceUrl("IAccounts", "SelectAccountByAuthenticationToken");
			var body = new JObject
				{
					 {"authenticationToken", AuthenticationToken }
				};

			Account = new HttpConnection().Post<Account>(url, body);
		}

		public void Logout()
		{
			DataModel.Settings.Update("marketplaceCredentials", null, null, null, null);

			IsLogged = false;
			Account = null;
		}

		private void TryLogin()
		{
			var loginInfo = DataModel.Settings.Select("marketplaceCredentials", null, null, null);

			if (loginInfo == null || string.IsNullOrWhiteSpace(loginInfo.Value))
				return;

			var decrypted = Shell.GetService<ISysCryptographyService>().Decrypt(this, loginInfo.Value);
			var tokens = decrypted.Split(new char[] { ' ' }, 2);

			try
			{
				Login(tokens[0], tokens[1]);
			}
			catch { }
		}

		public Guid SignUp(string company, string firstName, string lastName, string password, string email, string country, string phone, string website)
		{
			var url = new MarketplaceUrl("IAccounts", "InsertAccount");
			var body = new JObject
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

			return new HttpConnection().Post<Guid>(url, body);
		}

		public List<ICountry> QueryCountries()
		{
			var url = new MarketplaceUrl("ICountries", "QueryCountries");
			var ds = new HttpConnection().Get<JArray>(url);
			var r = new List<ICountry>();

			foreach (JObject i in ds)
			{
				r.Add(new Country
				{
					Name = i.Required<string>("name")
				});
			}

			return r;
		}

		public bool IsConfirmed(Guid accountKey)
		{
			var u = new MarketplaceUrl("IAccounts", "IsConfirmedAccount")
				 .AddParameter("accountKey", accountKey);

			return new HttpConnection().Get<bool>(u);
		}

		public ISubscriptionPlan SelectPlan(Guid token)
		{
			var u = new MarketplaceUrl("IPlans", "SelectPlan");
			var e = new JObject
			{
				{"token", token }
			};

			return new HttpConnection().Post<SubscriptionPlan>(u, e, new HttpRequestArgs().WithBasicCredentials(UserName, Password));
		}

		public List<ISubscriptionPlan> QuerySubscribedPlans()
		{
			var u = new MarketplaceUrl("IPlans", "QueryPlans");

			return new HttpConnection().Post<List<SubscriptionPlan>>(u, new HttpRequestArgs().WithBasicCredentials(UserName, Password)).ToList<ISubscriptionPlan>();
		}

		public List<ISubscriptionPlan> QueryMyPlans()
		{
			var u = new MarketplaceUrl("IPlans", "QueryMyPlans");

			return new HttpConnection().Post<List<SubscriptionPlan>>(u, new HttpRequestArgs().WithBasicCredentials(UserName, Password)).ToList<ISubscriptionPlan>();
		}

		public void PublishPackage(Guid microService)
		{
			var ms = DataModel.MicroServices.Select(microService);

			if (ms == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			if (ms.Package == Guid.Empty)
				throw new SysException(SR.ErrPackageNotCreated);


			var u = new MarketplaceUrl("IPackages", "PublishPackage");
			var e = new JObject
				{
					 {"data",  DataModel.BlobsContents.Select(ms.Package).Content}
				};

			new HttpConnection().Post(u, e, new HttpRequestArgs().WithBasicCredentials(UserName, Password));
		}

		public List<IPublishedPackage> QueryPackages(Guid plan)
		{
			var u = new MarketplaceUrl("IPackages", "QueryPackagesByPlan");
			var e = new JObject
				{
					{"plan", plan }
				};

			return new HttpConnection().Post<List<PublishedPackage>>(u, e, new HttpRequestArgs().WithBasicCredentials(UserName, Password)).ToList<IPublishedPackage>();
		}

		public IPublishedPackage SelectPublishedPackage(Guid microService, Guid plan)
		{
			var u = new MarketplaceUrl("IPackages", "SelectPackage");
			var e = new JObject
				{
					 {"microService", microService },
					 {"plan", plan }
				};

			return new HttpConnection().Post<PublishedPackage>(u, e, new HttpRequestArgs().WithBasicCredentials(UserName, Password));
		}

		public IPublishedPackage SelectPublishedPackage(Guid token)
		{
			var u = new MarketplaceUrl("IPackages", "SelectPackageByToken");
			var e = new JObject
				{
					 {"token", token }
				};

			return new HttpConnection().Post<PublishedPackage>(u, e, new HttpRequestArgs().WithBasicCredentials(UserName, Password));
		}

		public List<IPublishedPackage> QueryPublishedPackages(List<Tuple<Guid, Guid>> packages)
		{
			var u = new MarketplaceUrl("IPackages", "QueryPackages");
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

			return new HttpConnection().Post<List<PublishedPackage>>(u, e, new HttpRequestArgs().WithBasicCredentials(UserName, Password)).ToList<IPublishedPackage>();
		}

		public List<IInstallState> QueryInstallers()
		{
			return Shell.GetService<IDatabaseService>().Proxy.Deployment.QueryInstallers();
		}

		public void InsertInstallers(List<IInstallState> installers)
		{
			foreach (var i in installers)
			{
				var package = SelectPublishedPackage(i.Package);
				var ms = DataModel.MicroServices.SelectByPackage(i.Package);
				var type = InstallAuditType.Error;

				if (ms == null)
					type = InstallAuditType.PendingInstall;
				else
					type = InstallAuditType.PendingUpgrade;

				InsertInstallAudit(type, package.Token, null, new Version(package.Major, package.Minor, package.Build, package.Revision).ToString());
			}

			Shell.GetService<IDatabaseService>().Proxy.Deployment.Insert(installers);
		}

		private void InsertInstallAudit(InstallAuditType type, Guid package, string message, string version)
		{
			Shell.GetService<IDatabaseService>().Proxy.Deployment.InsertInstallAudit(type, package, DateTime.UtcNow, message, version);
		}

		public void UpdateInstaller(Guid package, InstallStateStatus status, string error)
		{
			if (!string.IsNullOrWhiteSpace(error))
				InsertInstallAudit(InstallAuditType.Error, package, error, ResolveLastPackageVersion(package));
			else
			{
				var ms = DataModel.MicroServices.SelectByPackage(package);
				var type = ms == null ? InstallAuditType.Installing : InstallAuditType.Upgrading;

				InsertInstallAudit(type, package, null, ResolveLastPackageVersion(package));
			}

			var item = Shell.GetService<IDatabaseService>().Proxy.Deployment.SelectInstaller(package);

			if (item == null)
				throw new SysException(SR.ErrInstallerNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Deployment.Update(item, status, error);
		}

		private string ResolveLastPackageVersion(Guid package)
		{
			var r = QueryInstallAudit(package).OrderByDescending(f => f.Created).Where(f => !string.IsNullOrWhiteSpace(f.Version));

			if (r.Count() == 0)
				return null;

			return r.First().Version;
		}

		public void DeleteInstaller(Guid package)
		{
			InsertInstallAudit(InstallAuditType.Complete, package, null, ResolveLastPackageVersion(package));

			var item = Shell.GetService<IDatabaseService>().Proxy.Deployment.SelectInstaller(package);

			if (item == null)
				throw new SysException(SR.ErrInstallerNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Deployment.Delete(item);
		}

		public byte[] DownloadPackage(Guid token)
		{
			var u = new MarketplaceUrl("IPackages", "DownloadPackage");
			var e = new JObject
				{
					 {"token", token }
				};

			return new HttpConnection().Post<byte[]>(u, e, new HttpRequestArgs().WithBasicCredentials(UserName, Password));
		}

		public byte[] DownloadConfiguration(Guid token)
		{
			var u = new MarketplaceUrl("IPackages", "SelectConfiguration");
			var e = new JObject
				{
					 {"token", token }
				};

			return new HttpConnection().Post<byte[]>(u, e, new HttpRequestArgs().WithBasicCredentials(UserName, Password));
		}

		internal List<IPackageVersion> CheckForUpdates(List<PackageVersion> packages)
		{
			var u = new MarketplaceUrl("IPackages", "CheckForUpdates");
			var e = new JObject();
			var a = new JArray();

			e.Add("packages", a);

			foreach (var i in packages)
			{
				a.Add(new JObject
					 {
						  {"microService", i.MicroService },
						  {"plan", i.Plan },
						  {"major", i.Major },
						  {"minor", i.Minor },
						  {"build", i.Build },
						  {"revision", i.Revision }
					 });
			}

			return new HttpConnection().Post<List<PackageVersion>>(u, e, new HttpRequestArgs().WithBasicCredentials(UserName, Password)).ToList<IPackageVersion>();
		}

		public Guid SelectInstallerConfiguration(Guid package)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Deployment.SelectInstallerConfiguration(package);
		}

		public void InsertInstallerConfiguration(Guid package, Guid configuration)
		{
			Shell.GetService<IDatabaseService>().Proxy.Deployment.InsertInstallerConfiguration(package, configuration);
		}

		public List<IInstallAudit> QueryInstallAudit(Guid package)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Deployment.QueryInstallAudit(package);
		}

		public List<IInstallAudit> QueryInstallAudit(DateTime from)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Deployment.QueryInstallAudit(from);
		}

		public List<IPackageDependency> QueryDependencies(Guid microService, Guid plan)
		{
			var u = new MarketplaceUrl("IPackages", "QueryDependencies");
			var e = new JObject
				{
					 {"microService", microService },
					 {"plan", plan }
				};

			return new HttpConnection().Post<List<PackageDependency>>(u, e, new HttpRequestArgs().WithBasicCredentials(UserName, Password)).ToList<IPackageDependency>();
		}

		public List<string> QueryTags()
		{
			var u = new MarketplaceUrl("ITags", "QueryTagList");

			return new HttpConnection().Get<List<string>>(u, new HttpRequestArgs().WithBasicCredentials(UserName, Password));
		}

		public List<ISubscription> QuerySubscriptions()
		{
			var u = new MarketplaceUrl("ISubscriptions", "QuerySubscriptions");

			return new HttpConnection().Get<List<Subscription>>(u, new HttpRequestArgs().WithBasicCredentials(UserName, Password)).ToList<ISubscription>();
		}
	}
}
