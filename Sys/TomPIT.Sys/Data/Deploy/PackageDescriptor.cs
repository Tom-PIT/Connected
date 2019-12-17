using System;
using TomPIT.Deployment;

namespace TomPIT.Sys.Data.Deploy
{
	internal class PackageDescriptor : IPackageDescriptor
	{
		public Guid MicroService { get; set; }

		public Guid Plan { get; set; }
	}
}
