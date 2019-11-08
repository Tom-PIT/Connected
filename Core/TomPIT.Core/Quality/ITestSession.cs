using System;

namespace TomPIT.Quality
{
	public interface ITestSession : ITestSessionEntity
	{
		Guid Suite { get; }
		Guid Token { get; }
	}
}
