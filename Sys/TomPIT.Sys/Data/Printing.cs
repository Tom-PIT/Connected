using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using TomPIT.Serialization;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Data
{
	internal class Printing
	{
		private const string Queue = "printing";
		public Guid Insert(Guid component, string provider, string arguments, string user)
		{
			var token = Guid.NewGuid();

			var message = new JObject
			{
				{ "id",token}
			};

			Shell.GetService<IDatabaseService>().Proxy.Printing.Insert(token, DateTime.UtcNow, component, PrintJobStatus.Pending, provider, arguments, user);
			DataModel.Queue.Enqueue(Queue, Serializer.Serialize(message), TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);

			return token;
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			return DataModel.Queue.Dequeue(count, TimeSpan.FromMinutes(4), QueueScope.System, Queue);
		}

		public void Ping(Guid popReceipt, TimeSpan nextVisible)
		{
			DataModel.Queue.Ping(popReceipt, nextVisible);
		}

		public void Complete(Guid popReceipt)
		{
			var m = DataModel.Queue.Select(popReceipt);

			if (m == null)
				return;

			DataModel.Queue.Complete(popReceipt);
			var job = ResolveJob(m);

			if (job != null)
				Delete(job.Token);
		}

		public void Error(Guid popReceipt, string error)
		{
			var m = DataModel.Queue.Select(popReceipt);

			if (m == null)
				return;

			DataModel.Queue.Complete(popReceipt);
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
