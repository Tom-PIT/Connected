using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using TomPIT.Api.Net;
using TomPIT.Services;
using TomPIT.Sys.Data;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Services
{
	internal class MessageDispatcher : HostedService
	{
		public MessageDispatcher()
		{
			IntervalTimeout = TimeSpan.FromSeconds(1);
		}
		protected override Task Process()
		{
			try
			{
				var ds = DataModel.MessageRecipients.QueryScheduled();

				foreach (var i in ds)
				{
					if (!Deliver(i))
						DataModel.MessageRecipients.Delete(i.Message, i.Connection);
				}
			}
			catch
			{
				//TODO: log exception
			}

			return Task.CompletedTask;
		}

		private bool Deliver(IRecipient recipient)
		{
			if (string.Compare(recipient.Topic, "cache", true) == 0)
				return DeliverCache(recipient);
			else
				throw new NotImplementedException();
		}

		private bool DeliverCache(IRecipient recipient)
		{
			var m = DataModel.Messages.Select(recipient.Message);

			if (m == null)
				return false;

			var state = JsonConvert.DeserializeObject<CacheState>(m.Text);

			if (state == null | string.IsNullOrWhiteSpace(state.Type))
				return false;

			var argsType = Type.GetType(state.Type, false);

			if (argsType == null)
				return false;

			object args = JsonConvert.DeserializeObject(state.Content, argsType);

			NotificationHubs.Cache.Clients.Client(recipient.Connection).SendCoreAsync(state.Method, new object[] { args });

			return true;
		}
	}
}
