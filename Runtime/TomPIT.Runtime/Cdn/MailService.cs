using System;
using System.Collections.Generic;
using TomPIT.Connectivity;

namespace TomPIT.Cdn
{
	internal class MailService : TenantObject, IMailService
	{
		public MailService(ITenant tenant) : base(tenant)
		{

		}

		public Guid Enqueue(string from, string to, string subject, string body, Dictionary<string, object> headers,
			int attachmentCount, MailFormat format, DateTime sendDate, DateTime expire)
		{
			return Instance.SysProxy.Mail.Enqueue(from, to, subject, body, headers, attachmentCount, format, sendDate, expire);
		}
	}
}
