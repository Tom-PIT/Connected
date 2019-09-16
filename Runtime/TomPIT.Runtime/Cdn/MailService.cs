using System;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	internal class MailService : TenantObject, IMailService
	{
		public MailService(ITenant tenant) : base(tenant)
		{

		}

		public Guid Enqueue(string from, string to, string subject, string body, JArray headers,
			int attachmentCount, MailFormat format, DateTime sendDate, DateTime expire)
		{
			var u = Tenant.CreateUrl("Mail", "Insert");
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

			return Tenant.Post<Guid>(u, e);
		}
	}
}
