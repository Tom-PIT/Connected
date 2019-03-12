using Newtonsoft.Json.Linq;
using System;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Cdn
{
	internal class MailService : ServiceBase, IMailService
	{
		public MailService(ISysConnection connection) : base(connection)
		{

		}

		public Guid Enqueue(string from, string to, string subject, string body, JArray headers, int attachmentCount, MailFormat format, DateTime sendDate, DateTime expire)
		{
			var u = Connection.CreateUrl("Mail", "Insert");
			var e = new JObject
			{
				{"from", from },
				{"to", to },
				{"subject", subject },
				{"body", body },
				{"format", format.ToString() },
				{"attachmentCount", attachmentCount },
				{"nextVisible", sendDate },
				{"expire", expire },
				{"headers", headers }
			};

			return Connection.Post<Guid>(u, e);
		}
	}
}
