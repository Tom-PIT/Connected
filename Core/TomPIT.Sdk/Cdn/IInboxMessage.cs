using System;
using System.Collections.Generic;

namespace TomPIT.Cdn
{
	public enum Importance
	{
		Low = 0,
		Normal = 1,
		High = 2
	}

	public enum Priority
	{
		NotUrgent = 0,
		Normal = 1,
		Urgent = 2
	}

	public enum XMessagePriority
	{
		Highest = 1,
		High = 2,
		Normal = 3,
		Low = 4,
		Lowest = 5
	}

	public interface IInboxMessage
	{
		long Size { get; }
		Importance Importance { get; }
		Priority Priority { get; }
		XMessagePriority XPriority { get; }
		IInboxAddress Sender { get; }
		IInboxAddress ResentSender { get; }
		IList<IInboxAddress> From { get; }
		IList<IInboxAddress> ResentFrom { get; }
		IList<IInboxAddress> ReplyTo { get; }
		IList<IInboxAddress> ResentReplyTo { get; }
		IList<IInboxAddress> To { get; }
		IList<IInboxAddress> ResentTo { get; }
		IList<IInboxAddress> Cc { get; }
		IList<IInboxAddress> ResentCc { get; }
		IList<IInboxAddress> Bcc { get; }
		IList<IInboxAddress> ResentBcc { get; }
		string Subject { get; }
		DateTimeOffset Date { get; }
		DateTimeOffset ResentDate { get; }
		List<string> References { get; }
		string InReplyTo { get; }
		string MessageId { get; }
		string ResentMessageId { get; }
		Version MimeVersion { get; }
		string Body { get; }
		List<IInboxHeader> Headers { get; }
		List<IInboxAttachment> Attachments { get; }
	}
}
