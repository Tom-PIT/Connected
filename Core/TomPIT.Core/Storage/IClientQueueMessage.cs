using System;

namespace TomPIT.Storage
{
	public interface IClientQueueMessage : IQueueMessage
	{
		Guid ResourceGroup { get; }
	}
}
