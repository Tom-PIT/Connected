using System;
using System.Collections.Generic;
using TomPIT.Cdn;

namespace TomPIT.Proxy
{
	public interface IMailController
	{
		Guid Enqueue(string from, string to, string subject, string body, Dictionary<string, object> headers, int attachmentCount, MailFormat format, DateTime sendDate, DateTime expire);
	}
}
