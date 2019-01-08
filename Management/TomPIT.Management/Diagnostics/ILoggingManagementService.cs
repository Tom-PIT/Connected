using System;
using System.Collections.Generic;
using TomPIT.Services;

namespace TomPIT.Diagnostics
{
	public interface ILoggingManagementService
	{
		void Clear(IExecutionContext sender);
		void Delete(IExecutionContext sender, long id);

		List<ILogEntry> Query(IExecutionContext sender, DateTime date);
	}
}
