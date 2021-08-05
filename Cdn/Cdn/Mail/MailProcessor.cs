using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MimeKit;
using MimeKit.Cryptography;
using TomPIT.Environment;
using TomPIT.Middleware;
using TomPIT.Storage;

namespace TomPIT.Cdn.Mail
{
	internal class MailProcessor
	{
		public MailProcessor(IMailMessage message, string resourceGroup)
		{
			Configuration = message;
			ResourceGroup = resourceGroup;
		}

		public void Create()
		{
			Message = new MimeMessage();

			CreateMeta();
			CreateHeaders();
			CreateBody();
			Sign();
		}

		public MimeMessage Message { get; private set; }
		private IMailMessage Configuration { get; }
		private string ResourceGroup { get; }

		private void CreateMeta()
		{
			Message.From.Add(MailboxAddress.Parse(Configuration.From));
			Message.To.Add(MailboxAddress.Parse(Configuration.To));
			Message.Subject = Configuration.Subject;

			if (!string.IsNullOrEmpty(SmtpService.DefaultEmailSender))
				Message.Sender = MailboxAddress.Parse(SmtpService.DefaultEmailSender);
			else
				Message.Sender = MailboxAddress.Parse(Configuration.From);
		}

		private void CreateHeaders()
		{
			if (string.IsNullOrWhiteSpace(Configuration.Headers))
				return;

			using (var sr = new StringReader(Configuration.Headers))
			{
				while (sr.Peek() != -1)
				{
					var line = sr.ReadLine();

					if (string.IsNullOrWhiteSpace(line))
						continue;

					var tokens = line.Split(new char[] { '=' }, 2);

					Message.Headers.Add(tokens[0], tokens[1]);

					if (string.Compare(tokens[0], Enum.GetName(typeof(HeaderId), HeaderId.References), true) == 0)
						Message.References.Add(tokens[1]);
				}
			}
		}

		private void CreateBody()
		{
			var builder = new BodyBuilder();
			using var ctx = new MiddlewareContext(MiddlewareDescriptor.Current.Tenant.Url);

			if (Configuration.Format == MailFormat.Html)
			{
				builder.HtmlBody = Configuration.Body;
				builder.TextBody = ctx.Services.Media.StripHtml(Configuration.Body);
			}
			else
				builder.TextBody = Configuration.Body;

			CreateAttachments(builder);

			Message.Body = builder.ToMessageBody();
		}

		private void Sign()
		{
			if (SmtpService.DkimPrivateKey.IsDefaultOrEmpty)
				return;

			using var ms = new MemoryStream(SmtpService.DkimPrivateKey.ToArray());

			var dkim = new DkimSigner(ms, SmtpService.DkimDomain, SmtpService.DkimSelector)
			{
				HeaderCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple,
				BodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple,
				AgentOrUserIdentifier = SmtpService.HostName,
			};

			var headers = new List<MimeKit.HeaderId>
			{
				MimeKit.HeaderId.From,
				MimeKit.HeaderId.Subject,
				MimeKit.HeaderId.To
			};

			if (Message.Sender is not null)
				headers.Add(MimeKit.HeaderId.Sender);

			Message.Prepare(EncodingConstraint.SevenBit);

			dkim.Sign(Message, headers);
		}

		private void CreateAttachments(BodyBuilder builder)
		{
			if (Configuration.AttachmentCount == 0)
				return;

			var rg = MiddlewareDescriptor.Current.Tenant.GetService<IResourceGroupService>().Select(ResourceGroup);
			var blobs = MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Query(Guid.Empty, BlobTypes.MailAttachment, rg.Token, Configuration.Token.ToString());

			foreach (var i in blobs)
			{
				var content = MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Download(i.Token);

				if (content == null)
					continue;

				using var ms = new MemoryStream(content.Content);

				builder.Attachments.Add(i.FileName, content.Content, ContentType.Parse(i.ContentType));
			}
		}

		public static InboxMessageResult ProcessMail(MemoryStream stream, List<AuthorizedRecipient> recipients)
		{
			if (stream == null || stream.Length == 0)
				return InboxMessageResult.Error;

			var context = new MessageContext(stream);

			if (context.Message == null)
				return InboxMessageResult.Error;

			var r = InboxMessageResult.MailboxNotFound;

			foreach (var recipient in recipients)
			{
				r = ProcessRecipient(context, recipient);

				if (r == InboxMessageResult.OK
					|| r == InboxMessageResult.MessageTooLarge
					|| r == InboxMessageResult.NotImplemented
					|| r == InboxMessageResult.AccessDenied
					|| r == InboxMessageResult.Error
					|| r == InboxMessageResult.SyntaxErrorInParameters
					|| r == InboxMessageResult.MailboxNotFound
					)
					break;
			}

			return r;
		}

		private static InboxMessageResult ProcessRecipient(MessageContext context, AuthorizedRecipient recipient)
		{
			return MiddlewareDescriptor.Current.Tenant.GetService<IInboxService>().ProcessMail(recipient.Email.Address, CreateInboxMessage(context.Message, context.Length));
		}

		private static IInboxMessage CreateInboxMessage(MimeMessage message, long length)
		{
			var result = new InboxMessage
			{
				Date = message.Date,
				Size = length,
				MessageId = message.MessageId,
				MimeVersion = message.MimeVersion,
				ResentDate = message.ResentDate,
				Subject = message.Subject,
				InReplyTo = message.InReplyTo,
				ResentMessageId = message.ResentMessageId,
				Body = string.IsNullOrWhiteSpace(message.HtmlBody) ? message.TextBody : message.HtmlBody
			};

			foreach (var header in message.Headers)
				result.Headers.Add(new InboxHeader(header.Field, header.Value));

			foreach (var part in message.Attachments)
			{
				if (!(part is MimePart mp))
					continue;

				using var ms = new MemoryStream();

				mp.Content.WriteTo(ms);

				ms.Seek(0, SeekOrigin.Begin);

				result.Attachments.Add(new InboxAttachment
				{
					Charset = part.ContentType.Charset,
					ContentType = part.ContentType.MimeType,
					MediaSubtype = part.ContentType.MediaSubtype,
					MediaType = part.ContentType.MediaType,
					Name = mp.FileName,
					Content = ms.ToArray()
				});
			}

			foreach (var address in message.Bcc)
				result.Bcc.Add(CreateAddress(address));

			foreach (var address in message.From)
				result.From.Add(CreateAddress(address));

			foreach (var address in message.To)
				result.To.Add(CreateAddress(address));

			foreach (var address in message.Cc)
				result.Cc.Add(CreateAddress(address));

			foreach (var address in message.ReplyTo)
				result.ReplyTo.Add(CreateAddress(address));

			foreach (var address in message.ResentBcc)
				result.ResentBcc.Add(CreateAddress(address));

			foreach (var address in message.ResentCc)
				result.ResentCc.Add(CreateAddress(address));

			foreach (var address in message.ResentFrom)
				result.ResentFrom.Add(CreateAddress(address));

			foreach (var address in message.ResentReplyTo)
				result.ResentReplyTo.Add(CreateAddress(address));

			if (message.ResentSender != null)
				result.ResentSender = CreateAddress(message.ResentSender);

			foreach (var address in message.ResentTo)
				result.ResentTo.Add(CreateAddress(address));

			if (message.Sender != null)
				result.Sender = CreateAddress(message.Sender);

			switch (message.Priority)
			{
				case MessagePriority.NonUrgent:
					result.Priority = Priority.NotUrgent;
					break;
				case MessagePriority.Normal:
					result.Priority = Priority.Normal;
					break;
				case MessagePriority.Urgent:
					result.Priority = Priority.Urgent;
					break;
				default:
					break;
			}

			switch (message.XPriority)
			{
				case MimeKit.XMessagePriority.Highest:
					result.XPriority = XMessagePriority.Highest;
					break;
				case MimeKit.XMessagePriority.High:
					result.XPriority = XMessagePriority.High;
					break;
				case MimeKit.XMessagePriority.Normal:
					result.XPriority = XMessagePriority.Normal;
					break;
				case MimeKit.XMessagePriority.Low:
					result.XPriority = XMessagePriority.Low;
					break;
				case MimeKit.XMessagePriority.Lowest:
					result.XPriority = XMessagePriority.Lowest;
					break;
				default:
					break;
			}

			switch (message.Importance)
			{
				case MessageImportance.Low:
					result.Importance = Importance.Low;
					break;
				case MessageImportance.Normal:
					result.Importance = Importance.Normal;
					break;
				case MessageImportance.High:
					result.Importance = Importance.High;
					break;
				default:
					break;
			}

			return result;
		}

		private static IInboxAddress CreateAddress(InternetAddress address)
		{
			var result = new InboxAddress
			{
				Name = address.Name
			};

			if (address is MailboxAddress mail)
				result.Address = mail.Address;

			return result;
		}
	}
}