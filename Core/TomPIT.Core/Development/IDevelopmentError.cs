using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Development
{
	public enum DevelopmentSeverity
	{
		Hidden = 0,
		Info = 1,
		Warning = 2,
		Error = 3
	}
	public interface IDevelopmentError: IDevelopmentComponentError
	{
		Guid MicroService { get; }
		Guid Component { get; }
		string ComponentName { get; }
		string ComponentCategory { get; }
	}
}
