using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Deployment;

namespace TomPIT.Proxy.Management
{
    public interface IDeploymentManagementController
    {
        bool SelectIsLogged();
        IAccount SelectAccount();
        void LogIn(string userName, string password);
        void LogOut();
        Guid SignUp(string company, string firstName, string lastName, string password, string email, string country, string phone, string website);

        bool IsConfirmed(Guid publisherKey);

        ImmutableList<ICountry> QueryCountries();

        byte[] DownloadPackage(Guid package);
        void PublishPackage(Guid microService);

        ImmutableList<ISubscriptionPlan> QuerySubscribedPlans();
        ISubscriptionPlan SelectPlan(Guid token);
        ImmutableList<ISubscriptionPlan> QueryMyPlans();

        ImmutableList<IPublishedPackage> QueryPackages(Guid plan);
        IPublishedPackage SelectPublishedPackage(Guid microService, Guid plan);
        IPublishedPackage SelectPublishedPackage(Guid token);
        ImmutableList<IPublishedPackage> QueryPublishedPackages(List<Tuple<Guid, Guid>> packages);

        void InsertInstallers(List<IInstallState> installers);
        ImmutableList<IInstallState> QueryInstallers();
        void UpdateInstaller(Guid package, InstallStateStatus status, string error);
        void DeleteInstaller(Guid package);

        ImmutableList<IPackageDependency> QueryDependencies(Guid microService, Guid plan);

        byte[] DownloadInstallerConfiguration(Guid package);
        void InsertInstallerConfiguration(Guid package, Guid configuration);
        Guid SelectInstallerConfigurationId(Guid package);

        ImmutableList<IInstallAudit> QueryInstallAudit(DateTime from);
        ImmutableList<IInstallAudit> QueryInstallAudit(Guid package);
        ImmutableList<IPackageVersion> CheckForUpdates(List<IPackageVersion> packages);

        ImmutableList<string> QueryTags();
        ImmutableList<ISubscription> QuerySubscriptions();
    }
}
