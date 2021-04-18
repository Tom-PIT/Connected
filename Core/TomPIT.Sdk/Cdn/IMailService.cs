using System;
using System.Collections.Generic;

namespace TomPIT.Cdn
{
	public interface IMailService
	{
		Guid Enqueue(string from, string to, string subject, string body, Dictionary<string, object> headers, int attachmentCount, MailFormat format, DateTime sendDate, DateTime expire);
	}
}
