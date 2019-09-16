using System;

namespace TomPIT.Quality
{
	public interface ITestSessionEntity
	{
		DateTime Start { get; }
		DateTime Complete { get; }
		TestRunStatus Status { get; }
		TestRunResult Result { get; }
		string Error { get; }
	}
}
