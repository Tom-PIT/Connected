using System;
using System.Collections.Generic;
using TomPIT.Analysis;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Model.Development
{
	public class ToolsModel
	{
		public List<ITool> Query()
		{
			return Shell.GetService<IDatabaseService>().Proxy.Development.Tools.Query();
		}

		public ITool Select(string name)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Development.Tools.Select(name);
		}

		public void Update(string name, ToolStatus status)
		{
			var existing = Select(name);

			if (existing != null)
			{
				if (existing.Status == status)
					return;

				if (existing.Status == ToolStatus.Running && status != ToolStatus.Idle)
					return;
				else if (existing.Status == ToolStatus.Pending && status != ToolStatus.Running)
					return;
			}

			var existingDate = existing == null ? DateTime.MinValue : existing.LastRun;

			Shell.GetService<IDatabaseService>().Proxy.Development.Tools.Update(name, status, status == ToolStatus.Running ? DateTime.UtcNow : existingDate);
		}
	}
}
