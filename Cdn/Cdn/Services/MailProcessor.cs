using MimeKit;
using System.IO;
using TomPIT.Storage;

namespace TomPIT.Cdn.Services
{
	internal class MailProcessor
	{
		public MailProcessor(IMailMessage message)
		{
			Configuration = message;
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
				}
			}
		}

		private void CreateBody()
		{
			var builder = new BodyBuilder();

			if (Configuration.Format == MailFormat.Html)
			{
				builder.HtmlBody = Configuration.Body;
				builder.TextBody = StringUtils.StripHtml(Configuration.Body);
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

			var blobs = Instance.Connection.GetService<IStorageService>().Query(Configuration.Token);

			foreach (var i in blobs)
			{
				var content = Instance.GetService<IStorageService>().Download(i.Token);

				if (content == null)
					continue;

				using (var ms = new MemoryStream(content.Content))
				{
					builder.Attachments.Add(MimeEntity.Load(ms));
				}
			}
		}
	}
}
