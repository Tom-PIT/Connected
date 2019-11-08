using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Analysis;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class ToolsHandler : IToolsHandler
	{
		public List<ITool> Query()
		{
			return new Reader<Tool>("tompit.dev_tool_state_que").Execute().ToList<ITool>();
		}

		public ITool Select(string name)
		{
			var r = new Reader<Tool>("tompit.dev_tool_state_sel");

			r.CreateParameter("@name", name);

			return r.ExecuteSingleRow();
		}

		public void Update(string name, ToolStatus status, DateTime lastRun)
		{
			var w = new Writer("tompit.dev_tool_state_mdf");

			w.CreateParameter("@name", name);
			w.CreateParameter("@status", status);
			w.CreateParameter("@last_run", lastRun, true);

			w.Execute();
		}
	}
}
