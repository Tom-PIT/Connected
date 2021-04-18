using System;
using System.Collections.Generic;

namespace TomPIT.Cdn.Mail
{
	internal class InboxMessage : IInboxMessage
	{
		private List<IInboxAddress> _from = null;
		private List<IInboxAddress> _resentFrom = null;
		private List<IInboxAddress> _replyTo = null;
		private List<IInboxAddress> _resentReplyTo = null;
		private List<IInboxAddress> _to = null;
		private List<IInboxAddress> _resentTo = null;
		private List<IInboxAddress> _cc = null;
		private List<IInboxAddress> _resentCc = null;
		private List<IInboxAddress> _bcc = null;
		private List<IInboxAddress> _resentBcc = null;

		private List<IInboxHeader> _headers = null;
		private List<IInboxAttachment> _attachments = null;
		public long Size { get; set; }

		public Importance Importance { get; set; }

		public Priority Priority { get; set; }

		public XMessagePriority XPriority { get; set; }

		public IInboxAddress Sender { get; set; }

		public IInboxAddress ResentSender { get; set; }

		public IList<IInboxAddress> From
		{
			get
			{
				if (_from == null)
					_from = new List<IInboxAddress>();

				return _from;
			}
		}

		public IList<IInboxAddress> ResentFrom
		{
			get
			{
				if (_resentFrom == null)
					_resentFrom = new List<IInboxAddress>();

				return _resentFrom;
			}
		}


		public IList<IInboxAddress> ReplyTo
		{
			get
			{
				if (_replyTo == null)
					_replyTo = new List<IInboxAddress>();

				return _replyTo;
			}
		}


		public IList<IInboxAddress> ResentReplyTo
		{
			get
			{
				if (_resentReplyTo == null)
					_resentReplyTo = new List<IInboxAddress>();

				return _resentReplyTo;
			}
		}


		public IList<IInboxAddress> To
		{
			get
			{
				if (_to == null)
					_to = new List<IInboxAddress>();

				return _to;
			}
		}


		public IList<IInboxAddress> ResentTo
		{
			get
			{
				if (_resentTo == null)
					_resentTo = new List<IInboxAddress>();

				return _resentTo;
			}
		}


		public IList<IInboxAddress> Cc
		{
			get
			{
				if (_cc == null)
					_cc = new List<IInboxAddress>();

				return _cc;
			}
		}


		public IList<IInboxAddress> ResentCc
		{
			get
			{
				if (_resentCc == null)
					_resentCc = new List<IInboxAddress>();

				return _resentCc;
			}
		}


		public IList<IInboxAddress> Bcc
		{
			get
			{
				if (_bcc == null)
					_bcc = new List<IInboxAddress>();

				return _bcc;
			}
		}


		public IList<IInboxAddress> ResentBcc
		{
			get
			{
				if (_resentBcc == null)
					_resentBcc = new List<IInboxAddress>();

				return _resentBcc;
			}
		}


		public string Subject { get; set; }

		public DateTimeOffset Date { get; set; }

		public DateTimeOffset ResentDate { get; set; }

		public List<string> References { get; set; }

		public string InReplyTo { get; set; }

		public string MessageId { get; set; }

		public string ResentMessageId { get; set; }

		public Version MimeVersion { get; set; }

		public string Body { get; set; }

		public List<IInboxHeader> Headers
		{
			get
			{
				if (_headers == null)
					_headers = new List<IInboxHeader>();

				return _headers;
			}
		}


		public List<IInboxAttachment> Attachments
		{
			get
			{
				if (_attachments == null)
					_attachments = new List<IInboxAttachment>();

				return _attachments;
			}
		}

	}
}
