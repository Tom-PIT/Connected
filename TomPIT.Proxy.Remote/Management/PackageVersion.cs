using System;
using TomPIT.Deployment;

namespace TomPIT.Proxy.Remote.Management;

internal class PackageVersion : IPackageVersion
{
    public Guid MicroService { get; set; }

    public Guid Plan { get; set; }

    public int Major { get; set; }

    public int Minor { get; set; }

    public int Build { get; set; }

    public int Revision { get; set; }
}
