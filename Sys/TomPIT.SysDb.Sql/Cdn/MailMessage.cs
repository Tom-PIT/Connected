using System;
using TomPIT.Cdn;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Cdn
{
	internal class MailMessage : LongPrimaryKeyRecord, IMailMessage
	{
		public DateTime Created { get; set; }
		public Guid Token { get; set; }
		public string From { get; set; }
		public string To { get; set; }
		public DateTime NextVisible { get; set; }
		public DateTime Expire { get; set; }
		public Guid PopReceipt { get; set; }
		public int DequeueCount { get; set; }
		public DateTime DequeueTimestamp { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public string Headers { get; set; }
		public int AttachmentCount { get; set; }
		public string Error { get; set; }
		public MailFormat Format { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Created = GetDate("created");
			Token = GetGuid("token");
			From = GetString("from");
			To = GetString("to");
			NextVisible = GetDate("next_visible");
			Expire = GetDate("expire");
			PopReceipt = GetGuid("pop_receipt");
			DequeueCount = GetInt("dequeue_count");
			DequeueTimestamp = GetDate("dequeue_timestamp");
			Subject = GetString("subject");
			Body = GetString("body");
			Headers = GetString("headers");
			AttachmentCount = GetInt("attachment_count");
			Error = GetString("error");
			Format = GetValue("format", MailFormat.Html);
		}
	}
}
