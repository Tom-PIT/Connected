using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Cdn;
using TomPIT.Serialization;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Caching;

namespace TomPIT.Sys.Model.Printing
{
	internal class PrintingModel : PersistentRepository<PrintJob, Guid>
	{
		private const string Queue = "printing";

		public PrintingModel(IMemoryCache container) : base(container, "printjob")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Printing.QueryJobs();

			foreach (var i in ds)
				Set(i.Token, new PrintJob(i), TimeSpan.Zero);
		}

		public Guid Insert(Guid component, string provider, string arguments, string user, string category, int copyCount)
		{
			var job = new PrintJob
			{
				CopyCount = Math.Max(copyCount, 1),
				Token = Guid.NewGuid(),
				SerialNumber = string.IsNullOrWhiteSpace(category) ? 0L : DataModel.PrintingSerialNumbers.Next(category),
				Component = component,
				Status = PrintJobStatus.Pending,
				Provider = provider,
				Arguments = arguments,
				Category = category,
				User = user
			};

			var message = new JObject
			{
				{ "id",job.Token}
			};

			Set(job.Token, job, TimeSpan.Zero);
			Dirty();

			DataModel.Queue.Enqueue(Queue, Serializer.Serialize(message), null, TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);

			return job.Token;
		}

		public void Delete(Guid token)
		{
			Remove(token);
			Dirty();
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

		public ImmutableList<IPrintQueueMessage> Dequeue(int count)
		{
			var messages = DataModel.Queue.Dequeue(count, TimeSpan.FromMinutes(4), QueueScope.System, Queue);

			if (!messages.Any())
				return ImmutableList<IPrintQueueMessage>.Empty;

			var result = new List<IPrintQueueMessage>();

			foreach (var message in messages)
			{
				if (JsonConvert.DeserializeObject(message.Message) is not JObject m || m.Required<Guid>("id") == Guid.Empty)
					continue;

				var id = m.Required<Guid>("id");

				if (id == Guid.Empty)
					continue;

				if (Select(id) is IPrintJob pj)
					result.Add(new PrintQueueMessage(message, pj));
				else
					DataModel.Queue.Complete(message.PopReceipt);
			}

			return result.ToImmutableList();
		}

		public void Ping(Guid popReceipt, TimeSpan nextVisible)
		{
			DataModel.Queue.Ping(popReceipt, nextVisible);
		}

		public void Complete(Guid popReceipt)
		{
			if (DataModel.Queue.Select(popReceipt) is not IQueueMessage message)
				return;

			DataModel.Queue.Complete(popReceipt);

			if (ResolveJob(message) is PrintJob job)
			{
				Remove(job.Token);
				Dirty();
			}
		}

		public void Error(Guid popReceipt, string error)
		{
			if (DataModel.Queue.Select(popReceipt) is not IQueueMessage message)
				return;

			DataModel.Queue.Ping(popReceipt, TimeSpan.FromSeconds(5));

			if (ResolveJob(message) is PrintJob job)
			{
				job.Status = PrintJobStatus.Error;
				job.Error = error;

				Dirty();
			}
		}

		public void Update(Guid token, PrintJobStatus status, string error)
		{
			if (Select(token) is not PrintJob job)
				return;

			job.Status = status;
			job.Error = error;

			Dirty();
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

		protected override async Task OnFlushing()
		{
			var items = All();

#if DEBUG
			var sw = new Stopwatch();

			sw.Start();
#endif
			Shell.GetService<IDatabaseService>().Proxy.Printing.Update(items.ToList<IPrintJob>());
			await Task.CompletedTask;
#if DEBUG
			sw.Stop();

			Debug.WriteLine($"Print jobs update {sw.ElapsedMilliseconds:n0} ms. {items.Count} items.", "Print jobs");
#endif
		}
	}
}
