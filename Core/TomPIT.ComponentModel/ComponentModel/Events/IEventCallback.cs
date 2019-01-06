using System;

namespace TomPIT.ComponentModel.Events
{
	public interface IEventCallback
	{
		Guid MicroService { get; }
		Guid Api { get; }
		Guid Operation { get; }
	}
}
