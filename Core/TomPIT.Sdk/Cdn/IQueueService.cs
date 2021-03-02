using System;
using TomPIT.ComponentModel.Distributed;

namespace TomPIT.Cdn
{
	public interface IQueueService
	{
		void Enqueue<T>(IQueueWorker worker, string bufferKey, T arguments);
		void Enqueue<T>(IQueueWorker worker, string bufferKey, T arguments, TimeSpan expire, TimeSpan nextVisible);

	}
}
