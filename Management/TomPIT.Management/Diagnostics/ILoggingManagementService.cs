using System;
using System.Collections.Generic;
using TomPIT.Diagnostics;

namespace TomPIT.Management.Diagnostics
{
	public interface ILoggingManagementService
	{
		void Clear();
		void Delete(long id);

		List<ILogEntry> Query(DateTime date);
		List<ILogEntry> Query(Guid metric);
	}
}
