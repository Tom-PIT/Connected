using System;
using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Data
{
	internal class Mail
	{
		public const int MailDequeueMax = 10;

		public void Clear()
		{
			Shell.GetService<IDatabaseService>().Proxy.Cdn.Mail.Clear();
		}

		public Guid Insert(string from, string to, DateTime nextVisible, DateTime expire, string subject, string body, string headers,
			int attachmentCount, MailFormat format)
		{
			var token = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.Cdn.Mail.Insert(token, DateTime.UtcNow, from, to, nextVisible, expire, subject, body, headers, attachmentCount, format);

			return token;
		}

		public void Response(Guid popReceipt, string error, int delay)
		{
			if (string.IsNullOrWhiteSpace(error))
			{
				Delete(popReceipt);
				return;
			}

			var message = SelectByPopReceipt(popReceipt);

			if (message == null)
				throw new SysException(SR.ErrMailMessageNotFound);

			if (message.DequeueCount >= MailDequeueMax || message.Expire < DateTime.UtcNow)
				Delete(message.Token);
			else
				Shell.GetService<IDatabaseService>().Proxy.Cdn.Mail.Update(popReceipt, error, DateTime.UtcNow.AddSeconds(delay));
		}

		public void Delete(Guid token)
		{
			var message = Select(token);

			if (message == null)
				throw new SysException(SR.ErrMailMessageNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Cdn.Mail.Delete(message);
		}

		public void DeleteByPopReceipt(Guid popReceipt)
		{
			Shell.GetService<IDatabaseService>().Proxy.Cdn.Mail.Delete(popReceipt);
		}

		public List<IMailMessage> Dequeue(int count)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Cdn.Mail.Dequeue(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(2), count);
		}

		public List<IMailMessage> Query()
		{
			return Shell.GetService<IDatabaseService>().Proxy.Cdn.Mail.Query();
		}

		public void Reset(Guid token)
		{
			var message = Select(token);

			if (message == null)
				throw new SysException(SR.ErrMailMessageNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Cdn.Mail.Reset(message);
		}

		public IMailMessage Select(Guid token)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Cdn.Mail.Select(token);
		}

		public IMailMessage SelectByPopReceipt(Guid popReceipt)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Cdn.Mail.SelectByPopReceipt(popReceipt);
		}

	}
}
