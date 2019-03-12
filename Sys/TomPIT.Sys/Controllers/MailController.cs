using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using TomPIT.Cdn;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class MailController : SysController
	{
		[HttpPost]
		public Guid Insert()
		{
			var body = FromBody();
			var from = body.Required<string>("from");
			var to = body.Required<string>("to");
			var nextVisible = body.Optional("nextVisible", DateTime.UtcNow);
			var expire = body.Optional("expire", DateTime.UtcNow.AddHours(24));
			var subject = body.Required<string>("subject");
			var mailBody = body.Optional("body", string.Empty);
			var headers = string.Empty;
			var a = body.Optional<JArray>("headers", null);

			if (a != null)
			{
				var sb = new StringBuilder();

				foreach (JObject header in a)
				{
					var property = header.First as JProperty;

					sb.AppendLine(string.Format("{0}={1}", property.Name, property.Value<string>()));
				}

				headers = sb.ToString();
			}

			var attachmentCount = body.Optional("attachmentCount", 0);
			var format = body.Optional("format", MailFormat.Html);

			return DataModel.Mail.Insert(from, to, nextVisible, expire, subject, mailBody, headers, attachmentCount, format);
		}
	}
}
