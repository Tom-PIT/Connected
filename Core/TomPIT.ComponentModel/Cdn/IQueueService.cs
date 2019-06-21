using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel.Workers;

namespace TomPIT.Cdn
{
	public interface IQueueService
	{
		void Enqueue<T>(IQueueWorker worker, T arguments);
		void Enqueue<T>(IQueueWorker worker, T arguments, TimeSpan expire, TimeSpan nextVisible);

	}
}
