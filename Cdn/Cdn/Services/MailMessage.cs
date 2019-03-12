using System;

namespace TomPIT.Cdn.Services
{
	internal class MailMessage : IMailMessage
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
	}
}
