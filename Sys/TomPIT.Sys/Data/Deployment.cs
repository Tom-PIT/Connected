using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Security;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Data
{
	public class Deployment
	{

		public bool IsLogged { get; private set; }
		public IAccount Account { get; private set; }
		private Guid AuthenticationToken { get; set; }
		private string UserName { get; set; }
		private string Password { get; set; }

		public Deployment()
		{
			TryLogin();
		}

		public void Login(string user, string password)
		{
			var url = new MarketplaceUrl("IIdentity", "Authenticate");

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

				var setting = DataModel.Settings.Select(Guid.Empty, "marketplaceCredentials");
				var value = Shell.GetService<ICryptographyService>().Encrypt(this, string.Format("{0} {1}", user, password));
				DataModel.Settings.Update(Guid.Empty, "marketplaceCredentials", value, false, DataType.Binary, null);

				LoadAccountProfile();
			}
			else
				throw new Exception(SR.ErrLoginFailed);
		}

		private void LoadAccountProfile()
		{
			var url = new MarketplaceUrl("IAccounts", "SelectByAuthenticationToken");
			var body = new JObject
			{
				{"token", AuthenticationToken }
			};

			Account = new HttpConnection().Post<Account>(url, body);
		}

		private void LoadAccountProfile(Guid key)
		{
			var url = new MarketplaceUrl("IAccount", "SelectByKey");
			var body = new JObject
			{
				{"accountKey", key }
			};

			Account = new HttpConnection().Post<Account>(url, body);
		}

		public void Logout()
		{
			DataModel.Settings.Update(Guid.Empty, "marketplaceCredentials", null, false, DataType.Binary, null);

			IsLogged = false;
			Account = null;
		}

		private void TryLogin()
		{
			var loginInfo = DataModel.Settings.Select(Guid.Empty, "marketplaceCredentials");

			if (loginInfo == null || string.IsNullOrWhiteSpace(loginInfo.Value))
				return;

			var decrypted = Shell.GetService<ICryptographyService>().Decrypt(this, loginInfo.Value);
			var tokens = decrypted.Split(new char[] { ' ' }, 2);

			try
			{
				Login(tokens[0], tokens[1]);
			}
			catch { }
		}

		public Guid SignUp(string company, string firstName, string lastName, string password, string email, int country, string phone, string website)
		{
			var url = new MarketplaceUrl("IPublishers", "Insert");
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
			var url = new MarketplaceUrl("ICountries", "Query");
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
			var u = new MarketplaceUrl("IAccounts", "IsConfirmed")
				.AddParameter("accountKey", accountKey);

			return new HttpConnection().Get<bool>(u);

		}

		public void PublishPackage(Guid microService)
		{
			var ms = DataModel.MicroServices.Select(microService);

			if (ms == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			if (ms.Package == Guid.Empty)
				throw new SysException(SR.ErrPackageNotCreated);


			var u = new MarketplaceUrl("IPackages", "Publish");
			var e = new JObject
			{
				{"data",  DataModel.BlobsContents.Select(ms.Package).Content}
			};

			new HttpConnection().Post(u, e, new HttpRequestArgs().WithBasicCredentials(UserName, Password));
		}

		public List<IPublishedPackage> QueryPublicPackages()
		{
			var u = new MarketplaceUrl("IPackages", "QueryPublic");

			return new HttpConnection().Get<List<PublishedPackage>>(u).ToList<IPublishedPackage>();
		}

		public IPublishedPackage SelectPublicPackage(Guid package)
		{
			var u = new MarketplaceUrl("IPackages", "Select");
			var e = new JObject
			{
				{"package", package }
			};

			return new HttpConnection().Post<PublishedPackage>(u, e, new HttpRequestArgs().WithBasicCredentials(UserName, Password));
		}

		public List<IInstallState> QueryInstallers()
		{
			return Shell.GetService<IDatabaseService>().Proxy.Deployment.QueryInstallers();
		}

		public void InsertInstallers(List<IInstallState> installers)
		{
			Shell.GetService<IDatabaseService>().Proxy.Deployment.Insert(installers);
		}

		public void UpdateInstaller(Guid package, InstallStateStatus status)
		{
			var item = Shell.GetService<IDatabaseService>().Proxy.Deployment.SelectInstaller(package);

			if (item == null)
				throw new SysException(SR.ErrInstallerNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Deployment.Update(item, status);
		}

		public void DeleteInstaller(Guid package)
		{
			var item = Shell.GetService<IDatabaseService>().Proxy.Deployment.SelectInstaller(package);

			if (item == null)
				throw new SysException(SR.ErrInstallerNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Deployment.Delete(item);
		}

		public byte[] DownloadPackage(Guid id)
		{
			var u = new MarketplaceUrl("IPackages", "Download");
			var e = new JObject
			{
				{"package", id }
			};

			return new HttpConnection().Post<byte[]>(u, e, new HttpRequestArgs().WithBasicCredentials(UserName, Password));
		}

		public byte[] DownloadConfiguration(Guid id)
		{
			var u = new MarketplaceUrl("IPackages", "SelectConfiguration");
			var e = new JObject
			{
				{"package", id }
			};

			return new HttpConnection().Post<byte[]>(u, e);
		}
	}
}
