using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using TomPIT.Serialization;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Data
{
	internal class Printing
	{
		private const string Queue = "printing";
		public Guid Insert(Guid component, string provider, string arguments)
		{
			var token = Guid.NewGuid();

			var message = new JObject
			{
				{ "id",token}
			};

			Shell.GetService<IDatabaseService>().Proxy.Printing.Insert(token, DateTime.UtcNow, component, PrintJobStatus.Pending, provider, arguments);
			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Enqueue(Queue, Serializer.Serialize(message), TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);

			return token;
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.DequeueSystem(Queue, count, TimeSpan.FromMinutes(4));
		}

		public void Ping(Guid popReceipt, TimeSpan nextVisible)
		{
			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Ping(popReceipt, nextVisible);
		}

		public void Complete(Guid popReceipt)
		{
			var m = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Select(popReceipt);

			if (m == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Delete(popReceipt);
			var job = ResolveJob(m);

			if (job != null)
				Delete(job.Token);
		}

		public void Error(Guid popReceipt, string error)
		{
			var m = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Select(popReceipt);

			if (m == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Delete(popReceipt);
			var job = ResolveJob(m);

			if (job != null)
				Update(job.Token, PrintJobStatus.Error, error);
		}

		public void Delete(Guid token)
		{
			Shell.GetService<IDatabaseService>().Proxy.Printing.Delete(token);
		}

		public void Update(Guid token, PrintJobStatus status, string error)
		{
			Shell.GetService<IDatabaseService>().Proxy.Printing.Update(token, status, error);
		}

		public IPrintJob Select(Guid token)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Printing.Select(token);
		}

		private IPrintJob ResolveJob(IQueueMessage message)
		{
			var d = Serializer.Deserialize<JObject>(message.Message);

			var id = d.Required<Guid>("id");

			return Select(id);
		}
	}
}
