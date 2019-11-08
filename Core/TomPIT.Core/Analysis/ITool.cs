using System;

namespace TomPIT.Analysis
{
	public enum ToolStatus
	{
		Idle = 1,
		Pending = 2,
		Running = 3
	}
	public interface ITool
	{
		string Name { get; }
		ToolStatus Status { get; }
		DateTime LastRun { get; }
	}
}
