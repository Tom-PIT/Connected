using System;
using TomPIT.Data;

namespace TomPIT.Cdn
{
	public enum MailFormat
	{
		Plain = 1,
		Html = 2
	}

	public interface IMailMessage : IPopReceiptRecord
	{
		DateTime Created { get; }
		Guid Token { get; }
		string From { get; }
		string To { get; }
		DateTime Expire { get; }
		int DequeueCount { get; }
		DateTime DequeueTimestamp { get; }
		string Subject { get; }
		string Body { get; }
		string Headers { get; }
		int AttachmentCount { get; }
		string Error { get; }
		MailFormat Format { get; }
	}
}
