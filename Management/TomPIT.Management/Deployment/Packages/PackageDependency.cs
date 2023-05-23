using System;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment.Packages;
internal class PackageDependency : IPackageDependency
{
    public string Title { get; set; }

    public Guid MicroService { get; set; }

    public Guid Plan { get; set; }
}
