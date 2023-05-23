using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Deployment;
using TomPIT.Proxy.Management;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management;
internal class DeploymentManagementController : IDeploymentManagementController
{
    public ImmutableList<IPackageVersion> CheckForUpdates(List<IPackageVersion> packages)
    {
        return DataModel.Deployment.CheckForUpdates(packages).ToImmutableList();
    }

    public void DeleteInstaller(Guid package)
    {
        DataModel.Deployment.DeleteInstaller(package);
    }

    public byte[] DownloadInstallerConfiguration(Guid package)
    {
        return DataModel.Deployment.DownloadConfiguration(package);
    }

    public byte[] DownloadPackage(Guid package)
    {
        return DataModel.Deployment.DownloadPackage(package);
    }

    public void InsertInstallerConfiguration(Guid package, Guid configuration)
    {
        DataModel.Deployment.InsertInstallerConfiguration(package, configuration);
    }

    public void InsertInstallers(List<IInstallState> installers)
    {
        DataModel.Deployment.InsertInstallers(installers);
    }

    public bool IsConfirmed(Guid publisherKey)
    {
        return DataModel.Deployment.IsConfirmed(publisherKey);
    }

    public void LogIn(string userName, string password)
    {
        DataModel.Deployment.Login(userName, password);
    }

    public void LogOut()
    {
        DataModel.Deployment.Logout();
    }

    public void PublishPackage(Guid microService)
    {
        DataModel.Deployment.PublishPackage(microService);
    }

    public ImmutableList<ICountry> QueryCountries()
    {
        return DataModel.Deployment.QueryCountries().ToImmutableList();
    }

    public ImmutableList<IPackageDependency> QueryDependencies(Guid microService, Guid plan)
    {
        return DataModel.Deployment.QueryDependencies(microService, plan).ToImmutableList();
    }

    public ImmutableList<IInstallAudit> QueryInstallAudit(DateTime from)
    {
        return DataModel.Deployment.QueryInstallAudit(from).ToImmutableList();
    }

    public ImmutableList<IInstallAudit> QueryInstallAudit(Guid package)
    {
        return DataModel.Deployment.QueryInstallAudit(package).ToImmutableList();
    }

    public ImmutableList<IInstallState> QueryInstallers()
    {
        return DataModel.Deployment.QueryInstallers().ToImmutableList();
    }

    public ImmutableList<ISubscriptionPlan> QueryMyPlans()
    {
        return DataModel.Deployment.QueryMyPlans().ToImmutableList();
    }

    public ImmutableList<IPublishedPackage> QueryPackages(Guid plan)
    {
        return DataModel.Deployment.QueryPackages(plan).ToImmutableList();
    }

    public ImmutableList<IPublishedPackage> QueryPublishedPackages(List<Tuple<Guid, Guid>> packages)
    {
        return DataModel.Deployment.QueryPublishedPackages(packages).ToImmutableList();
    }

    public ImmutableList<ISubscriptionPlan> QuerySubscribedPlans()
    {
        return DataModel.Deployment.QuerySubscribedPlans().ToImmutableList();
    }

    public ImmutableList<ISubscription> QuerySubscriptions()
    {
        return DataModel.Deployment.QuerySubscriptions().ToImmutableList();
    }

    public ImmutableList<string> QueryTags()
    {
        return DataModel.Deployment.QueryTags().ToImmutableList();
    }

    public IAccount SelectAccount()
    {
        return DataModel.Deployment.Account;
    }

    public Guid SelectInstallerConfigurationId(Guid package)
    {
        return DataModel.Deployment.SelectInstallerConfiguration(package);
    }

    public bool SelectIsLogged()
    {
        return DataModel.Deployment.IsLogged;
    }

    public ISubscriptionPlan SelectPlan(Guid token)
    {
        return DataModel.Deployment.SelectPlan(token);
    }

    public IPublishedPackage SelectPublishedPackage(Guid microService, Guid plan)
    {
        return DataModel.Deployment.SelectPublishedPackage(microService, plan);
    }

    public IPublishedPackage SelectPublishedPackage(Guid token)
    {
        return DataModel.Deployment.SelectPublishedPackage(token);
    }

    public Guid SignUp(string company, string firstName, string lastName, string password, string email, string country, string phone, string website)
    {
        return DataModel.Deployment.SignUp(company, firstName, lastName, password, email, country, phone, website);
    }

    public void UpdateInstaller(Guid package, InstallStateStatus status, string error)
    {
        DataModel.Deployment.UpdateInstaller(package, status, error);
    }
}
