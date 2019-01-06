using System;
using System.Collections.Generic;
using TomPIT.Runtime;

namespace TomPIT.Diagnostics
{
	public interface ILoggingManagementService
	{
		void Clear(IApplicationContext sender);
		void Delete(IApplicationContext sender, long id);

		List<ILogEntry> Query(IApplicationContext sender, DateTime date);
	}
}
