using System;
using System.Collections.Generic;
using TomPIT.Diagnostics;

namespace TomPIT.SysDb.Diagnostics
{
	public interface ILoggingHandler
	{
		void Insert(List<ILogEntry> items);

		void Clear();
		void Delete(long id);
		List<ILogEntry> Query(DateTime date);
		List<ILogEntry> Query(DateTime date, long metric);
		List<ILogEntry> Query(DateTime date, Guid component, Guid element);
	}
}
