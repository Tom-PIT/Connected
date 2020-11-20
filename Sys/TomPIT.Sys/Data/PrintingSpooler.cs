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
	internal class PrintingSpooler
	{
		private const string Queue = "printingSpooler";
		public Guid Insert(string mime, string printer, string content)
		{
			var token = Guid.NewGuid();

			var message = new JObject
			{
				{ "id",token},
				{ "printer", printer}
			};

			Shell.GetService<IDatabaseService>().Proxy.Printing.InsertSpooler(token, DateTime.UtcNow, mime, printer, content);
			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Enqueue(Queue, Serializer.Serialize(message), TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);

			return token;
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.DequeueSystem(Queue, count, TimeSpan.FromMinutes(30));
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

		public void Delete(Guid token)
		{
			var job = Select(token);

			if (job == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.Printing.DeleteSpooler(job);
		}

		public IPrintSpoolerJob Select(Guid token)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Printing.SelectSpooler(token);
		}

		private IPrintSpoolerJob ResolveJob(IQueueMessage message)
		{
			var d = Serializer.Deserialize<JObject>(message.Message);

			var id = d.Required<Guid>("id");

			return Select(id);
		}
	}
}
