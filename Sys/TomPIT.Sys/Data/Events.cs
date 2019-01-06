using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Api.Storage;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Environment;
using TomPIT.SysDb.Events;

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
			var sp = Shell.GetService<IStorageProviderService>().Select(rg.StorageProvider);

			var message = new JObject
			{
				{ "id",id}
			};

			Shell.GetService<IDatabaseService>().Proxy.Events.Insert(name, id, DateTime.UtcNow, a, callback);
			sp.Queue.Enqueue(rg, Queue, JsonConvert.SerializeObject(message), TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);

			return id;
		}

		public List<IClientQueueMessage> Dequeue(IServerResourceGroup resourceGroup, int count)
		{
			var provider = Shell.GetService<IStorageProviderService>().Select(resourceGroup.StorageProvider);

			if (provider == null)
				throw new SysException(string.Format("{0} ({1})", SR.ErrStorageProviderNotRegistered, resourceGroup.StorageProvider.ToString()));

			return provider.Queue.DequeueSystem(resourceGroup, Queue, count).ToClientQueueMessage(resourceGroup.Token);
		}

		public IEventDescriptor Select(Guid id)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Events.Select(id);
		}

		public void Complete(Guid resourceGroup, Guid popReceipt)
		{
			var sp = Shell.GetService<IStorageProviderService>().Resolve(resourceGroup);
			var res = DataModel.ResourceGroups.Select(resourceGroup);

			if (res == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			var m = sp.Queue.Select(res, popReceipt);

			if (m == null)
				return;

			var e = Resolve(m);

			if (e != null)
				Shell.GetService<IDatabaseService>().Proxy.Events.Delete(e);

			sp.Queue.Delete(res, popReceipt);
		}

		private IEventDescriptor Resolve(IQueueMessage message)
		{
			var d = JsonConvert.DeserializeObject(message.Message) as JObject;

			var id = d.Required<Guid>("id");

			return Shell.GetService<IDatabaseService>().Proxy.Events.Select(id);
		}
	}
}
