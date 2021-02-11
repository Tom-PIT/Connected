using System;
using TomPIT.Deployment;

namespace TomPIT.Sys.Model.Deployment
{
	internal class PackageDescriptor : IPackageDescriptor
	{
		public Guid MicroService { get; set; }

		public Guid Plan { get; set; }
	}
}
