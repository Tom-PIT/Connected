using System;
using TomPIT.Analysis;

namespace TomPIT.Ide.Analysis
{
	internal class Tool : ITool
	{
		public string Name { get; set; }

		public ToolStatus Status { get; set; }

		public DateTime LastRun { get; set; }
	}
}
