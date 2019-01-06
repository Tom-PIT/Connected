using System;
using System.Collections.Generic;

namespace TomPIT.Deployment
{
	public interface IDeploymentPackage
	{
		Guid Id { get; }
		string Name { get; }
		Version Version { get; }
		string Description { get; }

		IDeploymentMicroService MicroService { get; }
		List<IDeploymentFeature> Features { get; }
		List<IDeploymentComponent> Components { get; }
		List<IDeploymentString> Strings { get; }
		IDeploymentDatabase Database { get; }
		IDeploymentDependencies Dependencies { get; }
	}
}
