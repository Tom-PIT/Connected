using System;
using System.Collections.Generic;
using TomPIT.Environment;

namespace TomPIT.Services.Context
{
	public interface IContextEnvironmentService
	{
		List<IEnvironmentUnit> Query();
		List<IEnvironmentUnit> Query(Guid parent);
		IEnvironmentUnit Select(Guid environmentUnit);
	}
}
