using TomPIT.Deployment;

namespace TomPIT.Proxy.Management;

public enum PackageState
{
    NotInstalled = 1,
    Pending = 2,
    Installing = 3,
    Error = 4,
    Installed = 5,
    PendingUpgrade = 6,
    Upgrading = 7,
    UpgradeAvailable = 8
}

public interface IPackageStateDescriptor : IPublishedPackage
{
    PackageState State { get; set; }
}
