using System;

namespace TomPIT.Quality
{
	public enum TestRunStatus
	{
		Pending = 1,
		Running = 2,
		Completed = 3
	}

	public enum TestRunResult
	{
		Success = 1,
		Fail = 2
	}

	public interface ITestSuite
	{
		Guid Suite { get; }
		int RunCount { get; }
		int SuccessCount { get; }
		Guid MicroService { get; }
	}
}
