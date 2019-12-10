using System;
using TomPIT.Deployment;

namespace TomPIT.Sys.Data.Deploy
{
	internal class PackageDependency : IPackageDependency
	{
		public string Title { get; set; }

		public Guid MicroService { get; set; }

		public Guid Plan { get; set; }
	}
}
