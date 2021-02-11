using System;

namespace TomPIT.Cdn
{
	public enum PrintJobStatus
	{
		Pending = 1,
		Completed = 2,
		Error = 3
	}

	public interface IPrintJob
	{
		Guid Token { get; }
		PrintJobStatus Status { get; }
		string Error { get; }
		DateTime Created { get; }

		string Provider { get; set; }
		string Arguments { get; set; }
		Guid Component { get; set; }
		string User { get;  }
		long SerialNumber { get; }
		string Category { get; }
	}
}
