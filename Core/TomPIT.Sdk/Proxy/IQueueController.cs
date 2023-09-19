using System;

namespace TomPIT.Proxy
{
	public interface IQueueController
	{
		void Enqueue(Guid component, string name, string bufferKey, string arguments);
		void Enqueue(Guid component, string name, string bufferKey, string arguments, TimeSpan expire, TimeSpan nextVisible);
	}
}
