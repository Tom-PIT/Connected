﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Design.Serialization;
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

		public Guid SignUp(string company, string firstName, string lastName, string password, string email, int country, string phone, string website)
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

		public void CreatePackage(Guid microService, string name, string title, string version, PackageScope scope, bool trial, int trialPeriod,
			string description, double price, string tags, string projectUrl, string imageUrl, string licenseUrl, string licenses)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "CreatePackage");
			var ms = Connection.GetService<IMicroServiceService>().Select(microService);

			var package = new Package();
			var m = package.MetaData as PackageMetaData;

			m.Description = description;
			m.ImageUrl = imageUrl;
			m.LicenseUrl = licenseUrl;
			m.Name = name;
			m.Price = price;
			m.ProjectUrl = projectUrl;
			m.Scope = scope;
			m.Tags = tags;
			m.Title = title;
			m.Trial = trial;
			m.TrialPeriod = trialPeriod;
			m.Version = version;
			m.Licenses = licenses;
			m.Id = microService;
			m.Account = Account.Key;
			m.Created = DateTime.UtcNow;
			m.ShellVersion = Shell.Version.ToString();

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

			if (ms.Package != id)
				Connection.GetService<IMicroServiceManagementService>().Update(microService, ms.Name, ms.Status, ms.Template, ms.ResourceGroup, id, ms.Configuration);
		}

		public IPackage SelectPackage(Guid microService)
		{
			var ms = Connection.GetService<IMicroServiceService>().Select(microService);

			if (ms == null || ms.Package == null)
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
		}

		public List<IPublishedPackage> QueryPublicPackages()
		{
			var u = Connection.CreateUrl("DeploymentManagement", "QueryPublicPackages");

			return Connection.Get<List<PublishedPackage>>(u).ToList<IPublishedPackage>();
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

		public void UpdateInstaller(Guid package, InstallStateStatus status)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "UpdateInstaller");
			var e = new JObject
			{
				{"package", package },
				{"status", status.ToString() }
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

		public void Deploy(IPackage package)
		{
			new PackageDeployment(Connection, package).Deploy();
		}

		public IPublishedPackage SelectPublishedPackage(Guid package)
		{
			var u = Connection.CreateUrl("DeploymentManagement", "SelectPublishedPackage");
			var e = new JObject
			{
				{"package", package }
			};

			return Connection.Post<PublishedPackage>(u, e);
		}

		public IPackageConfiguration SelectPackageConfiguration(Guid package)
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

			if (ms == null || ms.Configuration == Guid.Empty)
				return config;

			var content = Connection.GetService<IStorageService>().Download(ms.Configuration);

			if (content == null || content.Content.Length == 0)
				return config;

			var existing = (PackageConfiguration)Connection.GetService<ISerializationService>().Deserialize(content.Content, typeof(PackageConfiguration));

			SynchronizeConfiguration(config, existing);

			return config;
		}

		public void UpdatePackageConfiguration(Guid microService, IPackageConfiguration configuration)
		{
			var ms = Connection.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			var id = Connection.GetService<IStorageService>().Upload(new Blob
			{
				ContentType = "application/json",
				FileName = string.Format("{0}.json", ms.Name),
				MicroService = microService,
				PrimaryKey = ms.Token.ToString(),
				Type = BlobTypes.PackageConfiguration,
				ResourceGroup = ms.ResourceGroup
			}, Connection.GetService<ISerializationService>().Serialize(configuration), StoragePolicy.Singleton);
		}

		private void SynchronizeConfiguration(PackageConfiguration configuration, PackageConfiguration existing)
		{
			configuration.ResourceGroup = existing.ResourceGroup;
			configuration.RuntimeConfiguration = existing.RuntimeConfiguration;

			foreach (var i in configuration.Databases)
			{
				var ed = existing.Databases.FirstOrDefault(f => f.Connection == i.Connection);

				if (ed == null)
					continue;

				var db = i as PackageConfigurationDatabase;

				db.ConnectionString = ed.ConnectionString;
			}
		}
	}
}