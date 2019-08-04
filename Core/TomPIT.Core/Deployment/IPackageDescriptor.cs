using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Deployment
{
	public interface IPackageDescriptor
	{
		Guid MicroService { get; }
		Guid Plan { get; }
	}
}
