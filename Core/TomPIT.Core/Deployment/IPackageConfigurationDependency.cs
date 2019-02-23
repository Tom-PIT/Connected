using System;

namespace TomPIT.Deployment
{
	public interface IPackageConfigurationDependency
	{
		Guid Dependency { get; }
		bool Enabled { get; }
	}
}
