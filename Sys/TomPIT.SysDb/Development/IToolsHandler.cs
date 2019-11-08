using System;
using System.Collections.Generic;
using TomPIT.Analysis;

namespace TomPIT.SysDb.Development
{
	public interface IToolsHandler
	{
		void Update(string name, ToolStatus status, DateTime lastRun);
		ITool Select(string name);
		List<ITool> Query();
	}
}
