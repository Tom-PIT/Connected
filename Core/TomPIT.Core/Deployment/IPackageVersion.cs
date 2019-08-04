using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Deployment
{
	public interface IPackageVersion
	{
		Guid MicroService { get; }
		Guid Plan { get; }
		int Major { get; }
		int Minor { get; }
		int Build { get; }
		int Revision { get; }
	}
}
