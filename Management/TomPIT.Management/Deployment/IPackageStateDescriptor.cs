using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Deployment;
using TomPIT.Management.Designers;

namespace TomPIT.Management.Deployment
{
	public interface IPackageStateDescriptor : IPublishedPackage
	{
		PackageState State { get; set; }
	}
}
