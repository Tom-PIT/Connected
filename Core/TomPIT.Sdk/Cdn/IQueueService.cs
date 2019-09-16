using System;
using TomPIT.ComponentModel.Distributed;

namespace TomPIT.Cdn
{
	public interface IQueueService
	{
		void Enqueue<T>(IQueueConfiguration handler, T arguments);
		void Enqueue<T>(IQueueConfiguration handler, T arguments, TimeSpan expire, TimeSpan nextVisible);

	}
}
