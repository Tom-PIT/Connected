using System;
using System.Threading.Tasks;
using TomPIT.Services;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Services
{
	internal class MessageDisposer : HostedService
	{
		public MessageDisposer()
		{
			IntervalTimeout = TimeSpan.FromSeconds(15);
		}

		protected override Task Process()
		{
			try
			{
				var ds = DataModel.Messages.Query();

				foreach (var i in ds)
				{
					if (i.Expire < DateTime.UtcNow)
					{
						DataModel.Messages.Delete(i.Token);
						continue;
					}

					var recipients = DataModel.MessageRecipients.Query(i.Token);

					if (recipients.Count == 0)
						DataModel.Messages.Delete(i.Token);
				}

				var subscribers = DataModel.MessageSubscribers.Query();

				foreach (var i in subscribers)
				{
					if (i.Alive.AddMinutes(5) < DateTime.UtcNow)
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