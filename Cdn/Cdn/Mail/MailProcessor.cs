using System;
using System.Collections.Generic;
using System.IO;
using MimeKit;
using TomPIT.Environment;
using TomPIT.Middleware;
using TomPIT.Storage;

namespace TomPIT.Cdn.Mail
{
	internal enum MailProcessorResult
	{
		Cancelled = 0,
		OK = 250,
		SyntaxErrorInParameters = 501, //Syntax error in parameters or arguments
		MailboxNotFound = 550,//Requested action not taken: mailbox unavailable
		AccessDenied = 530,//Access denied
		MessageTooLarge = 552,//Requested action aborted: exceeded storage allocation
		Error = 451,//Requested action not taken: local error in processing
		NotImplemented = 502//Command not implemented
	}

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
		}

		public MimeMessage Message { get; private set; }
		private IMailMessage Configuration { get; }
		private string ResourceGroup { get; }

		private void CreateMeta()
		{
			Message.From.Add(new MailboxAddress(Configuration.From));
			Message.To.Add(new MailboxAddress(Configuration.To));
			Message.Subject = Configuration.Subject;
			Message.Sender = new MailboxAddress(Configuration.From);
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
			var ctx = new MiddlewareContext(MiddlewareDescriptor.Current.Tenant.Url);

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

		public static MailProcessorResult ProcessMail(MemoryStream stream, List<AuthorizedRecipient> recipients)
		{
			if (stream == null || stream.Length == 0)
				return MailProcessorResult.Error;

			var context = new MessageContext(stream);

			if (context.Message == null)
				return MailProcessorResult.Error;

			var r = MailProcessorResult.MailboxNotFound;

			foreach (var recipient in recipients)
			{
				r = ProcessRecipient(context, recipient);

				if (r == MailProcessorResult.OK
					|| r == MailProcessorResult.MessageTooLarge
					|| r == MailProcessorResult.NotImplemented
					|| r == MailProcessorResult.AccessDenied)
					break;
			}

			return r;
		}

		private static MailProcessorResult ProcessRecipient(MessageContext context, AuthorizedRecipient recipient)
		{
			return MailProcessorResult.OK;
		}
	}
}