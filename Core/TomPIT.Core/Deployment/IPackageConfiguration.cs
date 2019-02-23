using System;
using System.Collections.Generic;

namespace TomPIT.Deployment
{
	public interface IPackageConfiguration
	{
		Guid ResourceGroup { get; }
		bool RuntimeConfigurationSupported { get; }
		bool RuntimeConfiguration { get; }
		bool AutoVersioning { get; }
		List<IPackageConfigurationDatabase> Databases { get; }
		List<IPackageConfigurationDependency> Dependencies { get; }
	}
}
