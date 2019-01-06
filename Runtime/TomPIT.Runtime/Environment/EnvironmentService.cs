using System;
using System.Collections.Generic;
using TomPIT.Environment;
using TomPIT.Sys.Environment;

namespace TomPIT.Runtime.Environment
{
	internal class EnvironmentService : IEnvironmentService
	{
		public string InstanceEndpointUrl(InstanceType type, InstanceVerbs verb)
		{
			throw new NotImplementedException();
		}

		public List<IEnvironmentUnit> QueryEnvironmentUnits()
		{
			throw new NotImplementedException();
		}

		public List<IEnvironmentUnit> QueryEnvironmentUnits(Guid parent)
		{
			throw new NotImplementedException();
		}

		public List<IInstanceEndpoint> QueryInstanceEndpoints()
		{
			throw new NotImplementedException();
		}

		public List<IInstanceEndpoint> QueryInstanceEndpoints(InstanceType type)
		{
			throw new NotImplementedException();
		}

		public List<IResourceGroup> QueryResourceGroups(IApplicationContext context)
		{
			return EnvironmentApi.QueryResourceGroups(context);
		}

		public IEnvironmentUnit SelectEnvironmentUnit(Guid token)
		{
			throw new NotImplementedException();
		}

		public IInstanceEndpoint SelectInstanceEndpoint(Guid token)
		{
			throw new NotImplementedException();
		}

		public IInstanceEndpoint SelectInstanceEndpoint(InstanceType type)
		{
			throw new NotImplementedException();
		}

		public IResourceGroup SelectResourceGroup(Guid token)
		{
			throw new NotImplementedException();
		}
	}
}
