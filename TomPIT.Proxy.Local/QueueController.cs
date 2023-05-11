using System;
using TomPIT.Serialization;
using TomPIT.Sys.Model;
using TomPIT.Sys.Model.Cdn;

namespace TomPIT.Proxy.Local
{
	internal class QueueController : IQueueController
	{

		public void Enqueue(Guid component, string name, string bufferKey, string arguments)
		{
			Enqueue(component, name, bufferKey, arguments);
		}

		public void Enqueue(Guid component, string name, string bufferKey, string arguments, TimeSpan expire, TimeSpan nextVisible)
		{
			var message = new
			{
				component,
				worker = name
			};

			DataModel.Queue.Enqueue(QueueingModel.Queue, Serializer.Serialize(message), bufferKey, expire, nextVisible, Storage.QueueScope.Content);
		}
	}
}
