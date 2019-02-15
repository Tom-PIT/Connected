using System;
using System.Collections.Generic;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	public interface IDeploymentService
	{
		bool IsLogged { get; }
		IAccount Account { get; }
		void LogIn(string userName, string password);
		void LogOut();
		Guid SignUp(string company, string firstName, string lastName, string password, string email, int country, string phone, string website);

		bool IsConfirmed(Guid publisherKey);

		List<ICountry> QueryCountries();

		void CreatePackage(Guid microService, string name, string title, string version, PackageScope scope, bool trial, int trialPeriod,
			string description, double price, string tags, string projectUrl, string imageUrl, string licenseUrl, string licenses);

		IPackage SelectPackage(Guid microService);
		IPackage DownloadPackage(Guid package);
		void PublishPackage(Guid microService);

		List<IPublishedPackage> QueryPublicPackages();
		IPublishedPackage SelectPublishedPackage(Guid package);

		void InsertInstallers(List<IInstallState> installers);
		List<IInstallState> QueryInstallers();
		void UpdateInstaller(Guid package, InstallStateStatus status);
		void DeleteInstaller(Guid package);


		void Deploy(IPackage package);

		IPackageConfiguration SelectPackageConfiguration(Guid package);
		void UpdatePackageConfiguration(Guid microService, IPackageConfiguration configuration);
	}
}
