using System;
using TomPIT.Deployment;

namespace TomPIT.Sys.Model.Deployment
{
	internal class PackageDependency : IPackageDependency
	{
		public string Title { get; set; }

		public Guid MicroService { get; set; }

		public Guid Plan { get; set; }
	}
}
