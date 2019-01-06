using System;

namespace TomPIT.Runtime
{
	public interface IExecutionContext
	{
		int Event { get; }
		string Authority { get; }
		string Id { get; }
		string Property { get; }
		Guid MicroService { get; }
	}
}
