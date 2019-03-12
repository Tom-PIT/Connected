using System;

namespace TomPIT.QA
{
	public interface ITestSession : ITestSessionEntity
	{
		Guid Suite { get; }
		Guid Token { get; }
	}
}
