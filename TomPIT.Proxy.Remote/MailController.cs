using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Cdn;

namespace TomPIT.Proxy.Remote
{
	internal class MailController : IMailController
	{
		private const string Controller = "Mail";

		public Guid Enqueue(string from, string to, string subject, string body, Dictionary<string, object> headers, int attachmentCount, MailFormat format, DateTime sendDate, DateTime expire)
		{
			var e = new Dictionary<string, object>
			{
				{"from", from },
				{"to", to },
				{"subject", subject },
				{"body", body },
				{"format", format.ToString() },
				{"attachmentCount", attachmentCount },
				{"nextVisible", sendDate },
				{"expire", expire }
			};

			if (headers is not null && headers.Any())
			{
				var items = new Dictionary<string, object>();

				e.Add("headers", items);

				foreach (var header in headers)
					items.Add(header.Key, header.Value);
			}

			return Connection.Post<Guid>(Connection.CreateUrl(Controller, "Insert"), e);
		}
	}
}
