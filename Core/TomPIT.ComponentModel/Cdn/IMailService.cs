using Newtonsoft.Json.Linq;
using System;

namespace TomPIT.Cdn
{
	public interface IMailService
	{
		Guid Enqueue(string from, string to, string subject, string body, JArray headers, int attachmentCount, MailFormat format, DateTime sendDate, DateTime expire);
	}
}
