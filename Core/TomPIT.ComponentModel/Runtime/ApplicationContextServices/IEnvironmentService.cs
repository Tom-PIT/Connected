using System;
using System.Collections.Generic;
using TomPIT.Environment;

namespace TomPIT.Runtime.ApplicationContextServices
{
	public interface IEnvironmentService
	{
		List<IEnvironmentUnit> Query();
		List<IEnvironmentUnit> Query(Guid parent);
		IEnvironmentUnit Select(Guid environmentUnit);
	}
}
