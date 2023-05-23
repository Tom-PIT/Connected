using System;
using TomPIT.Deployment;

namespace TomPIT.Proxy.Remote.Management;

internal class PackageDependency : IPackageDependency
{
    public string Title { get; set; }

    public Guid MicroService { get; set; }

    public Guid Plan { get; set; }
}
