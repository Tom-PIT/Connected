using System;
using System.Collections.Generic;

namespace TomPIT.Environment
{
	public interface IEnvironmentUnitService
	{
		List<IEnvironmentUnit> Query();
		List<IEnvironmentUnit> Query(Guid parent);
		IEnvironmentUnit Select(Guid environmentUnit);
	}
}
