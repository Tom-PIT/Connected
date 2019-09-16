using System;

namespace TomPIT.Development
{
	public enum ErrorCategory
	{
		NotSet = 0,
		Syntax = 1,
		Validation = 2,
		Type = 3
	}

	public enum DevelopmentSeverity
	{
		Hidden = 0,
		Info = 1,
		Warning = 2,
		Error = 3
	}

	public interface IDevelopmentError
	{
		Guid Component { get; }
		Guid Element { get; }
		DevelopmentSeverity Severity { get; }
		string Message { get; }
		int Code { get; }
		ErrorCategory Category { get; }
		Guid Identifier { get; }
	}
}
