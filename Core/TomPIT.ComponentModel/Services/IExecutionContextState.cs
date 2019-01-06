using System;

namespace TomPIT.Services
{
	public interface IExecutionContextState
	{
		int Event { get; }
		string Authority { get; }
		string Id { get; }
		string Property { get; }
		Guid MicroService { get; }
	}
}
