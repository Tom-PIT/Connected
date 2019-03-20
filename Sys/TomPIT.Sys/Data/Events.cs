using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Environment;
using TomPIT.SysDb.Events;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Data
{
	internal class Events
	{
		private const string Queue = "event";

		public Guid Insert(Guid microService, string name, JObject e, string callback)
		{
			var id = Guid.NewGuid();
			var a = e == null ? string.Empty : JsonConvert.SerializeObject(e);

			var ms = microService == Guid.Empty
				? null
				: DataModel.MicroServices.Select(microService);

			if (microService != Guid.Empty && ms == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			var rg = DataModel.ResourceGroups.Select(ms == null ? Guid.Empty : ms.ResourceGroup);

			var message = new JObject
			{
				{ "id",id}
			};

			Shell.GetService<IDatabaseService>().Proxy.Events.Insert(name, id, DateTime.UtcNow, a, callback);
			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Enqueue(rg, Queue, JsonConvert.SerializeObject(message), TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);

			return id;
		}

		public List<IClientQueueMessage> Dequeue(IServerResourceGroup resourceGroup, int count)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.DequeueSystem(resourceGroup, Queue, count).ToClientQueueMessage(resourceGroup.Token);
		}

		public IEventDescriptor Select(Guid id)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Events.Select(id);
		}

		public void Ping(Guid resourceGroup, Guid popReceipt)
		{
			var res = DataModel.ResourceGroups.Select(resourceGroup);

			if (res == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			var m = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Select(res, popReceipt);

			if (m == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Ping(res, popReceipt, TimeSpan.FromSeconds(5));
		}

		public void Complete(Guid resourceGroup, Guid popReceipt)
		{
			var res = DataModel.ResourceGroups.Select(resourceGroup);

			if (res == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			var m = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Select(res, popReceipt);

			if (m == null)
				return;

			var e = Resolve(m);

			if (e != null)
				Shell.GetService<IDatabaseService>().Proxy.Events.Delete(e);

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Delete(res, popReceipt);
		}

		private IEventDescriptor Resolve(IQueueMessage message)
		{
			var d = JsonConvert.DeserializeObject(message.Message) as JObject;

			var id = d.Required<Guid>("id");

			return Shell.GetService<IDatabaseService>().Proxy.Events.Select(id);
		}
	}
}
