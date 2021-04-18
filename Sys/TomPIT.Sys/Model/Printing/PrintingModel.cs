using System;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Cdn;
using TomPIT.Serialization;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Model.Printing
{
	internal class PrintingModel: SynchronizedRepository<IPrintJob, Guid>
	{
		private const string Queue = "printing";

		public PrintingModel(IMemoryCache container) : base(container, "printjob")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Printing.QueryJobs();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Printing.Select(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public Guid Insert(Guid component, string provider, string arguments, string user, string category)
		{
			var token = Guid.NewGuid();

			var serialNumber = 0L;

			if (!string.IsNullOrWhiteSpace(category))
				serialNumber = DataModel.PrintingSerialNumbers.Next(category);

			var message = new JObject
			{
				{ "id",token}
			};

			Shell.GetService<IDatabaseService>().Proxy.Printing.Insert(token, DateTime.UtcNow, component, PrintJobStatus.Pending, provider, arguments, user, serialNumber, category);

			Refresh(token);

			DataModel.Queue.Enqueue(Queue, Serializer.Serialize(message), null, TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);

			return token;
		}

		public long NextSerialNumber(string category)
		{
			if (string.IsNullOrWhiteSpace(category))
				return -1;

			var ordered = Where(f => string.Compare(f.Category, category, true) == 0).OrderBy(f => f.SerialNumber);

			if (ordered.Count() == 0)
				return -1;

			return ordered.First().SerialNumber;
		}

		public ImmutableList<IQueueMessage> Dequeue(int count)
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
			Remove(token);
			Shell.GetService<IDatabaseService>().Proxy.Printing.Delete(token);
		}

		public void Update(Guid token, PrintJobStatus status, string error)
		{
			Shell.GetService<IDatabaseService>().Proxy.Printing.Update(token, status, error);
			Refresh(token);
		}

		public IPrintJob Select(Guid token)
		{
			return Get(token);
		}

		private IPrintJob ResolveJob(IQueueMessage message)
		{
			var d = Serializer.Deserialize<JObject>(message.Message);

			var id = d.Required<Guid>("id");

			return Select(id);
		}
	}
}
