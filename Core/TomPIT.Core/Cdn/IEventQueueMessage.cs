using System;
using TomPIT.Storage;

namespace TomPIT.Cdn
{
	public interface IEventQueueMessage : IQueueMessage
	{
		string Arguments { get; }
		string Name { get; }
		string Callback { get; }
		Guid MicroService { get; }
	}
}
