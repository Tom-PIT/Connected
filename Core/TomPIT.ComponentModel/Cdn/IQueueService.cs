using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel.Handlers;

namespace TomPIT.Cdn
{
	public interface IQueueService
	{
		void Enqueue<T>(IQueueHandlerConfiguration handler, T arguments);
		void Enqueue<T>(IQueueHandlerConfiguration handler, T arguments, TimeSpan expire, TimeSpan nextVisible);

	}
}
