using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.Deployment;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Management
{
	public class DeploymentManagementController : SysController
	{

		[HttpGet]
		public bool IsLogged()
		{
			return DataModel.Deployment.IsLogged;
		}

		[HttpGet]
		public IAccount SelectAccount()
		{
			return DataModel.Deployment.Account;
		}

		[HttpPost]
		public void Login()
		{
			var body = FromBody();
			var user = body.Required<string>("user");
			var password = body.Required<string>("password");

			DataModel.Deployment.Login(user, password);
		}

		[HttpPost]
		public void Logout()
		{
			DataModel.Deployment.Logout();
		}

		[HttpPost]
		public Guid Signup()
		{
			var body = FromBody();
			var company = body.Required<string>("company");
			var firstName = body.Required<string>("firstName");
			var lastName = body.Required<string>("lastName");
			var password = body.Required<string>("password");
			var email = body.Required<string>("email");
			var country = body.Required<int>("country");
			var phone = body.Optional("phone", string.Empty);
			var website = body.Optional("webSite", string.Empty);

			return DataModel.Deployment.SignUp(company, firstName, lastName, password, email, country, phone, website);
		}

		[HttpPost]
		public List<ICountry> QueryCountries()
		{
			return DataModel.Deployment.QueryCountries();
		}

		[HttpPost]
		public bool IsConfirmed()
		{
			var body = FromBody();
			var key = body.Required<Guid>("accountKey");

			return DataModel.Deployment.IsConfirmed(key);
		}

		[HttpPost]
		public void PublishPackage()
		{
			var body = FromBody();
			var microService = body.Required<Guid>("microService");

			DataModel.Deployment.PublishPackage(microService);
		}

		[HttpGet]
		public List<IPublishedPackage> QueryPublicPackages()
		{
			return DataModel.Deployment.QueryPublicPackages();
		}

		[HttpPost]
		public IPublishedPackage SelectPublishedPackage()
		{
			var body = FromBody();
			var package = body.Required<Guid>("package");

			return DataModel.Deployment.SelectPublicPackage(package);
		}

		[HttpPost]
		public void InsertInstallers()
		{
			var body = FromBody();
			var packages = new List<IInstallState>();
			var items = body.Required<JArray>("installers");

			foreach (JObject i in items)
			{
				packages.Add(new InstallState
				{
					Package = i.Required<Guid>("package"),
					Parent = i.Optional("parent", Guid.Empty)
				});
			}

			DataModel.Deployment.InsertInstallers(packages);
		}

		[HttpPost]
		public void UpdateInstaller()
		{
			var body = FromBody();
			var package = body.Required<Guid>("package");
			var status = body.Required<InstallStateStatus>("status");

			DataModel.Deployment.UpdateInstaller(package, status);
		}

		[HttpPost]
		public void DeleteInstaller()
		{
			var body = FromBody();
			var package = body.Required<Guid>("package");

			DataModel.Deployment.DeleteInstaller(package);
		}

		[HttpGet]
		public List<IInstallState> QueryInstallers()
		{
			return DataModel.Deployment.QueryInstallers();
		}

		[HttpPost]
		public byte[] DownloadPackage()
		{
			var body = FromBody();
			var package = body.Required<Guid>("package");

			return DataModel.Deployment.DownloadPackage(package);
		}

		[HttpPost]
		public byte[] DownloadConfiguration()
		{
			var body = FromBody();
			var package = body.Required<Guid>("package");

			return DataModel.Deployment.DownloadConfiguration(package);
		}

		[HttpPost]
		public Guid SelectInstallerConfiguration()
		{
			var body = FromBody();
			var package = body.Required<Guid>("package");

			return DataModel.Deployment.SelectInstallerConfiguration(package);
		}

		[HttpPost]
		public void InsertInstallerConfiguration()
		{
			var body = FromBody();
			var package = body.Required<Guid>("package");
			var configuration = body.Required<Guid>("configuration");

			DataModel.Deployment.InsertInstallerConfiguration(package, configuration);
		}
	}
}
