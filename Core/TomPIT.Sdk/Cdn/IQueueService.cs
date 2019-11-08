using System;
using TomPIT.ComponentModel.Distributed;

namespace TomPIT.Cdn
{
	public interface IQueueService
	{
		void Enqueue<T>(IQueueWorker worker, T arguments);
		void Enqueue<T>(IQueueWorker worker, T arguments, TimeSpan expire, TimeSpan nextVisible);

	}
}
