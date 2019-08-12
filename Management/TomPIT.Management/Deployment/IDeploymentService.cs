using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	public interface IDeploymentService
	{
		bool IsLogged { get; }
		IAccount Account { get; }
		void LogIn(string userName, string password);
		void LogOut();
		Guid SignUp(string company, string firstName, string lastName, string password, string email, string country, string phone, string website);

		bool IsConfirmed(Guid publisherKey);

		List<ICountry> QueryCountries();

		void CreatePackage(Guid microService, Guid plan, string name, string title, string version, string description, string tags, string projectUrl, string imageUrl, string licenseUrl, string licenses, bool runtimeConfigurationSupported, bool autoVersion);

		IPackage SelectPackage(Guid microService);
		IPackage DownloadPackage(Guid package);
		void PublishPackage(Guid microService);

		List<ISubscriptionPlan> QuerySubscribedPlans();
		ISubscriptionPlan SelectPlan(Guid token);
		List<ISubscriptionPlan> QueryMyPlans();

		List<IPackageStateDescriptor> QueryPackages(Guid plan);
		IPublishedPackage SelectPublishedPackage(Guid microService, Guid plan);
		IPublishedPackage SelectPublishedPackage(Guid token);
		List<IPublishedPackage> QueryPublishedPackages(List<Tuple<Guid, Guid>> packages);

		void InsertInstallers(List<IInstallState> installers);
		List<IInstallState> QueryInstallers();
		void UpdateInstaller(Guid package, InstallStateStatus status, string error);
		void DeleteInstaller(Guid package);

		void Deploy(Guid id, IPackage package);

		List<IPackageDependency> QueryDependencies(Guid microService, Guid plan);

		IPackageConfiguration SelectInstallerConfiguration(Guid package);
		void UpdateInstallerConfiguration(Guid package, IPackageConfiguration configuration);

		List<IInstallAudit> QueryInstallAudit(DateTime from);
		List<IInstallAudit> QueryInstallAudit(Guid package);
		List<IPackageVersion> CheckForUpdates(List<IMicroService> microServices);

		List<string> QueryTags();
		List<ISubscription> QuerySubscriptions();
	}
}