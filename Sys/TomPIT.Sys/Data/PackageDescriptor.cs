using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Deployment;

namespace TomPIT.Sys.Data
{
	internal class PackageDescriptor : IPackageDescriptor
	{
		public Guid MicroService {get;set;}

		public Guid Plan {get;set;}
	}
}
