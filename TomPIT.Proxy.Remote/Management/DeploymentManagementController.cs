using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Deployment;
using TomPIT.Proxy.Management;

namespace TomPIT.Proxy.Remote.Management;
internal class DeploymentManagementController : IDeploymentManagementController
{
    private const string Controller = "DeploymentManagement";

    public IAccount SelectAccount()
    {
        return Connection.Get<Account>(Connection.CreateUrl(Controller, "SelectAccount"));
    }
    public ImmutableList<IPackageVersion> CheckForUpdates(List<IPackageVersion> packages)
    {
        return Connection.Post<List<PackageVersion>>(Connection.CreateUrl(Controller, "CheckForUpdates"), new
        {
            packages
        }).ToImmutableList<IPackageVersion>();
    }

    public void DeleteInstaller(Guid package)
    {
        Connection.Post(Connection.CreateUrl(Controller, "DeleteInstaller"), new
        {
            package
        });
    }

    public byte[] DownloadPackage(Guid package)
    {
        return Connection.Post<byte[]>(Connection.CreateUrl(Controller, "DownloadPackage"), new
        {
            package
        });
    }

    public void InsertInstallers(List<IInstallState> installers)
    {
        Connection.Post(Connection.CreateUrl(Controller, "InsertInstallers"), new
        {
            installers
        });
    }

    public bool IsConfirmed(Guid publisherKey)
    {
        return Connection.Post<bool>(Connection.CreateUrl(Controller, "IsConfirmed"), new
        {
            accountKey = publisherKey
        });
    }

    public void LogIn(string userName, string password)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Login"), new
        {
            userName,
            password
        });
    }

    public void LogOut()
    {
        Connection.Post(Connection.CreateUrl(Controller, "Logout"));
    }

    public void PublishPackage(Guid microService)
    {
        Connection.Post(Connection.CreateUrl(Controller, "PublishPackage"), new
        {
            microService
        });
    }

    public ImmutableList<ICountry> QueryCountries()
    {
        return Connection.Get<List<Country>>(Connection.CreateUrl(Controller, "QueryCountries")).ToImmutableList<ICountry>();
    }

    public ImmutableList<IPackageDependency> QueryDependencies(Guid microService, Guid plan)
    {
        return Connection.Post<List<PackageDependency>>(Connection.CreateUrl(Controller, "QueryDependencies"), new
        {
            microService,
            plan
        }).ToImmutableList<IPackageDependency>();
    }

    public ImmutableList<IInstallAudit> QueryInstallAudit(DateTime from)
    {
        return Connection.Post<List<InstallAudit>>(Connection.CreateUrl(Controller, "QueryInstallAudit"), new
        {
            from
        }).ToImmutableList<IInstallAudit>();
    }

    public ImmutableList<IInstallAudit> QueryInstallAudit(Guid package)
    {
        return Connection.Post<List<InstallAudit>>(Connection.CreateUrl(Controller, "QueryInstallAudit"), new
        {
            package
        }).ToImmutableList<IInstallAudit>();
    }

    public ImmutableList<IInstallState> QueryInstallers()
    {
        return Connection.Get<List<InstallState>>(Connection.CreateUrl(Controller, "QueryInstallers")).ToImmutableList<IInstallState>();
    }

    public ImmutableList<ISubscriptionPlan> QueryMyPlans()
    {
        return Connection.Get<List<SubscriptionPlan>>(Connection.CreateUrl(Controller, "QueryMyPlans")).ToImmutableList<ISubscriptionPlan>();
    }

    public ImmutableList<IPublishedPackage> QueryPackages(Guid plan)
    {
        return Connection.Post<List<PublishedPackage>>(Connection.CreateUrl(Controller, "QueryPackages"), new
        {
            plan
        }).ToImmutableList<IPublishedPackage>();
    }

    public ImmutableList<IPublishedPackage> QueryPublishedPackages(List<Tuple<Guid, Guid>> packages)
    {
        var items = new List<dynamic>();

        foreach (var item in packages)
        {
            items.Add(new
            {
                MicroService = item.Item1,
                Plan = item.Item2
            });
        }

        return Connection.Post<List<PublishedPackage>>(Connection.CreateUrl(Controller, "QueryPublishedPackages"), new
        {
            packages = items
        }).ToImmutableList<IPublishedPackage>();
    }

    public ImmutableList<ISubscriptionPlan> QuerySubscribedPlans()
    {
        return Connection.Get<List<SubscriptionPlan>>(Connection.CreateUrl(Controller, "QuerySubscribedPlans")).ToImmutableList<ISubscriptionPlan>();
    }

    public ImmutableList<ISubscription> QuerySubscriptions()
    {
        return Connection.Get<List<Subscription>>(Connection.CreateUrl(Controller, "QuerySubscriptions")).ToImmutableList<ISubscription>();
    }

    public ImmutableList<string> QueryTags()
    {
        return Connection.Get<List<string>>(Connection.CreateUrl(Controller, "QueryTags")).ToImmutableList();
    }

    public byte[] DownloadInstallerConfiguration(Guid package)
    {
        return Connection.Post<byte[]>(Connection.CreateUrl(Controller, "DownloadConfiguration"), new
        {
            package
        });
    }

    public bool SelectIsLogged()
    {
        return Connection.Get<bool>(Connection.CreateUrl(Controller, "IsLogged"));
    }

    public ISubscriptionPlan SelectPlan(Guid token)
    {
        return Connection.Post<SubscriptionPlan>(Connection.CreateUrl(Controller, "SelectPlan"), new
        {
            token
        });
    }

    public IPublishedPackage SelectPublishedPackage(Guid microService, Guid plan)
    {
        return Connection.Post<PublishedPackage>(Connection.CreateUrl(Controller, "SelectPublishedPackage"), new
        {
            microService,
            plan
        });
    }

    public IPublishedPackage SelectPublishedPackage(Guid token)
    {
        return Connection.Post<PublishedPackage>(Connection.CreateUrl(Controller, "SelectPublishedPackageByToken"), new
        {
            token
        });
    }

    public Guid SignUp(string company, string firstName, string lastName, string password, string email, string country, string phone, string website)
    {
        return Connection.Post<Guid>(Connection.CreateUrl(Controller, "Signup"), new
        {
            company,
            firstName,
            lastName,
            password,
            email,
            country,
            phone,
            website
        });
    }

    public void UpdateInstaller(Guid package, InstallStateStatus status, string error)
    {
        Connection.Post(Connection.CreateUrl(Controller, "UpdateInstaller"), new
        {
            package,
            error,
            status = status.ToString()
        });
    }

    public void InsertInstallerConfiguration(Guid package, Guid configuration)
    {
        Connection.Post(Connection.CreateUrl(Controller, "InsertInstallerConfiguration"), new
        {
            package,
            configuration
        });
    }

    public Guid SelectInstallerConfigurationId(Guid package)
    {
        return Connection.Post<Guid>(Connection.CreateUrl(Controller, "SelectInstallerConfiguration"), new
        {
            package
        });
    }
}
