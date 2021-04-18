using System;
using System.Collections.Generic;
using TomPIT.Diagnostics;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Model.Diagnostics
{
	public class LoggingModel
	{
		public static int Avatar = 1;

		public const string CategoryWorker = "Worker";

		public void Insert(List<ILogEntry> items)
		{
			Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Logging.Insert(items);
		}

		public void Clear()
		{
			Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Logging.Clear();
		}

		public void Delete(long id)
		{
			Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Logging.Delete(id);
		}

		public List<ILogEntry> Query(DateTime date)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Logging.Query(date);
		}

		public List<ILogEntry> Query(Guid metric)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Logging.Query(metric);
		}

		public List<ILogEntry> Query(DateTime date, Guid component, Guid element)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Diagnostics.Logging.Query(date, component, element);
		}
	}
}
