using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.Cdn;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class MailController : IMailController
	{
		public Guid Enqueue(string from, string to, string subject, string body, Dictionary<string, object> headers, int attachmentCount, MailFormat format, DateTime sendDate, DateTime expire)
		{
			var headersString = new StringBuilder();

			if (headers is not null && headers.Any())
			{
				foreach (var header in headers)
					headersString.AppendLine($"{header.Key}={header.Value}");
			}

			return DataModel.Mail.Insert(from, to, sendDate, expire, subject, body, headersString.ToString(), attachmentCount, format);
		}
	}
}
