using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Sys.Model;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Services
{
	internal class MessageDisposer : HostedService
	{
		public MessageDisposer()
		{
			IntervalTimeout = TimeSpan.FromSeconds(15);
		}

		protected override bool OnInitialize(CancellationToken cancel)
		{
			return DataModel.Initialized;
		}
		protected override Task OnExecute(CancellationToken cancel)
		{
			try
			{
				var ds = DataModel.Messages.Query();

				var deleted = new List<IMessage>();
				var deletedRecps = new List<IRecipient>();

				foreach (var i in ds)
				{
					if (i.Expire < DateTime.UtcNow)
					{
						deleted.Add(i);

						var recps = DataModel.MessageRecipients.Query(i.Token);

						if (recps.Count > 0)
							deletedRecps.AddRange(recps);

						continue;
					}

					var recipients = DataModel.MessageRecipients.Query(i.Token);

					if (recipients.Count == 0)
						deleted.Add(i);
				}

				if (deleted.Count > 0)
					DataModel.Messages.Clean(deleted, deletedRecps);

				var subscribers = DataModel.MessageSubscribers.Query();

				foreach (var i in subscribers)
				{
					if (i.Alive.AddSeconds(90) < DateTime.UtcNow)
						DataModel.MessageSubscribers.Delete(i.Topic, i.Connection);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				//TODO: log exception
			}


			return Task.CompletedTask;
		}
	}
}