using LZ4;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TomPIT.ComponentModel;
using TomPIT.Configuration;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Design.Serialization;
using TomPIT.Security;
using TomPIT.Storage;

namespace TomPIT.Marketplace
{
	internal class MarketplaceService : IMarketplaceService
	{
		private const string MarketplaceUrl = "http://localhost/marketplace/rest";

		public MarketplaceService(ISysConnection connection)
		{
			Connection = connection;

			TryLogin();
		}

		private void TryLogin()
		{
			var loginInfo = Connection.GetService<ISettingService>().GetValue<string>(Guid.Empty, "marketplaceCredentials");

			if (string.IsNullOrWhiteSpace(loginInfo))
				return;

			var decrypted = Connection.GetService<ICryptographyService>().Decrypt(loginInfo);
			var tokens = decrypted.Split(new char[] { ' ' }, 2);

			try
			{
				LogIn(tokens[0], tokens[1], true);
			}
			catch { }
		}

		private ISysConnection Connection { get; }
		private string UserName { get; set; }
		private string Password { get; set; }

		public void LogOut()
		{
			Connection.GetService<ISettingManagementService>().Update(Guid.Empty, "marketplaceCredentials", null, false, DataType.Binary, null);

			IsLogged = false;
			Publisher = null;
		}

		public void LogIn(string userName, string password, bool permanent)
		{
			var url = Connection.CreateUrl(MarketplaceUrl, "Marketplace", "IIdentity", "Authenticate");
			var body = new JObject
			{
				{"user", userName },
				{"password", password }
			};

			var ar = Connection.Post<Guid>(url, body);

			if (ar != Guid.Empty)
			{
				IsLogged = true;
				AuthenticationToken = ar;
				UserName = userName;
				Password = password;

				var setting = Connection.GetService<ISettingService>().Select(Guid.Empty, "marketplaceCredentials");

				var value = Connection.GetService<ICryptographyService>().Encrypt(string.Format("{0} {1}", userName, password));

				Connection.GetService<ISettingManagementService>().Update(Guid.Empty, "marketplaceCredentials", value, false, DataType.Binary, null);

				LoadPublisherProfile();
			}
			else
				throw new Exception(SR.ErrLoginFailed);
		}

		private void LoadPublisherProfile()
		{
			var url = Connection.CreateUrl(MarketplaceUrl, "Marketplace", "IPublishers", "SelectByAuthenticationToken");
			var body = new JObject
			{
				{"token", AuthenticationToken }
			};

			var ds = Connection.Post<JObject>(url, body);

			if (!ds.IsEmpty())
				Publisher = JsonConvert.DeserializeObject<Publisher>(JsonConvert.SerializeObject(ds.FirstResult()));
		}

		private void LoadPublisherProfile(Guid key)
		{
			var url = Connection.CreateUrl(MarketplaceUrl, "Marketplace", "IPublishers", "SelectByKey");
			var body = new JObject
			{
				{"publisherKey", key }
			};

			Publisher = Connection.Post<Publisher>(url, body);
		}

		public Guid SignUp(string company, string firstName, string lastName, string password, string email, int country, string phone, string website)
		{
			var url = Connection.CreateUrl(MarketplaceUrl, "Marketplace", "IPublishers", "Insert");
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

			return Connection.Post<Guid>(url, body);
		}

		public JObject QueryCountries()
		{
			var url = Connection.CreateUrl(MarketplaceUrl, "Marketplace", "ICountries", "Query");

			return Connection.Get<JObject>(url);
		}

		public bool IsConfirmed(Guid publisherKey)
		{
			var u = Connection.CreateUrl(MarketplaceUrl, "Marketplace", "IPublishers", "IsConfirmed")
				.AddParameter("publisherKey", publisherKey);

			return Connection.Get<bool>(u);

		}

		public void CreatePackage(Guid microService, string name, string title, string version, PackageScope scope, bool trial, int trialPeriod,
			string description, double price, string tags, string projectUrl, string imageUrl, string licenseUrl, string licenses, PackageProcessHandler processCallback)
		{
			var md = new PackageMetaData
			{
				Id = microService,
				Created = DateTime.Today,
				Description = description,
				ImageUrl = imageUrl,
				LicenseUrl = licenseUrl,
				Name = name,
				Price = price,
				ProjectUrl = projectUrl,
				Publisher = Publisher.Key,
				Scope = scope,
				ShellVersion = Shell.Version.ToString(),
				Tags = tags,
				Title = title,
				Trial = trial,
				TrialPeriod = trialPeriod,
				Version = version,
				Licenses = licenses
			};

			var package = Package.Create(new PackageCreateArgs(Connection, microService, md, processCallback));
			var ms = Connection.GetService<IMicroServiceService>().Select(microService);

			var blob = new Blob
			{
				ContentType = "application/json",
				FileName = string.Format("{0}.json", ms.Name),
				MicroService = microService,
				PrimaryKey = microService.ToString(),
				ResourceGroup = ms.ResourceGroup,
				Type = BlobTypes.Package,
			};

			var content = Connection.GetService<ISerializationService>().Serialize(package);
			var id = Connection.GetService<IStorageService>().Upload(blob, content, StoragePolicy.Singleton);

			if (ms.Package != id)
				Connection.GetService<IMicroServiceManagementService>().Update(microService, ms.Name, ms.Status, ms.Template, ms.ResourceGroup, id);
		}

		public IPackage SelectPackage(Guid microService)
		{
			var ms = Connection.GetService<IMicroServiceService>().Select(microService);

			if (ms == null || ms.Package == null)
				return null;

			var content = Connection.GetService<IStorageService>().Download(ms.Package);

			if (content == null || content.Content == null || content.Content.Length == 0)
				return null;

			return (Package)Connection.GetService<ISerializationService>().Deserialize(content.Content, typeof(Package));
		}

		public void PublishPackage(Guid microService)
		{
			var package = SelectPackage(microService);

			if (package == null)
				throw new RuntimeException(SR.ErrPackageNotFound);

			var u = Connection.CreateUrl(MarketplaceUrl, "Marketplace", "IPackages", "Publish");
			var e = new JObject
			{
				{"data",  LZ4Codec.Wrap(Connection.GetService<ISerializationService>().Serialize(package))}
			};

			Connection.Post(u, e, new HttpRequestArgs().WithBasicCredentials(UserName, Password));
		}

		public bool IsLogged { get; private set; }
		public IPublisher Publisher { get; private set; }

		private Guid AuthenticationToken { get; set; }
	}
}
