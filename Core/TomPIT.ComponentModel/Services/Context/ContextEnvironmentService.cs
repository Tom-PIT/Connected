using System;
using System.Collections.Generic;
using TomPIT.Environment;

namespace TomPIT.Services.Context
{
	internal class ContextEnvironmentService : ContextClient, IContextEnvironmentService
	{
		public ContextEnvironmentService(IExecutionContext context) : base(context)
		{
		}

		public List<IEnvironmentUnit> Query()
		{
			return Context.Connection().GetService<IEnvironmentUnitService>().Query();
		}

		public List<IEnvironmentUnit> Query(Guid parent)
		{
			return Context.Connection().GetService<IEnvironmentUnitService>().Query(parent);
		}

		public IEnvironmentUnit Select(Guid environmentUnit)
		{
			return Context.Connection().GetService<IEnvironmentUnitService>().Select(environmentUnit);
		}
	}
}
