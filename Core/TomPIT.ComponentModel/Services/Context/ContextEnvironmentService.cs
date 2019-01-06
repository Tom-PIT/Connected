using System;
using System.Collections.Generic;
using TomPIT.Environment;

namespace TomPIT.Services.Context
{
	internal class ContextEnvironmentService : IContextEnvironmentService
	{
		public ContextEnvironmentService(IExecutionContext context)
		{
			Context = context;
		}

		private IExecutionContext Context { get; }

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
