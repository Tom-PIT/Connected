using System;
using System.Collections.Generic;
using TomPIT.Environment;

namespace TomPIT.Runtime.ApplicationContextServices
{
	internal class EnvironmentService : IEnvironmentService
	{
		public EnvironmentService(IApplicationContext context)
		{
			Context = context;
		}

		private IApplicationContext Context { get; }

		public List<IEnvironmentUnit> Query()
		{
			return Context.GetServerContext().GetService<IEnvironmentUnitService>().Query();
		}

		public List<IEnvironmentUnit> Query(Guid parent)
		{
			return Context.GetServerContext().GetService<IEnvironmentUnitService>().Query(parent);
		}

		public IEnvironmentUnit Select(Guid environmentUnit)
		{
			return Context.GetServerContext().GetService<IEnvironmentUnitService>().Select(environmentUnit);
		}
	}
}
