using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel.Workers;

namespace TomPIT.Cdn
{
	public interface IQueueService
	{
		void Enqueue(IQueueWorker worker, JObject arguments);
		void Enqueue(IQueueWorker worker, JObject arguments, TimeSpan expire, TimeSpan nextVisible);

	}
}
