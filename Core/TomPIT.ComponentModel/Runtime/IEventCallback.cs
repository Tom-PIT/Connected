using System;

namespace TomPIT.Runtime
{
	public interface IEventCallback
	{
		Guid MicroService { get; }
		Guid Api { get; }
		Guid Operation { get; }
	}
}
