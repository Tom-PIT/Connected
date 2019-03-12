using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Cdn;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Cdn;

namespace TomPIT.SysDb.Sql.Cdn
{
	internal class MailHandler : IMailHandler
	{
		public void Clear()
		{
			new Writer("tompit.mail_clr").Execute();
		}

		public void Delete(IMailMessage message)
		{
			var w = new Writer("tompit.mail_del");

			w.CreateParameter("@id", message.GetId());

			w.Execute();
		}

		public void Delete(Guid popReceipt)
		{
			var w = new Writer("tompit.mail_queue_del");

			w.CreateParameter("@pop_receipt", popReceipt);

			w.Execute();
		}

		public List<IMailMessage> Dequeue(DateTime date, DateTime nextVisible, int count)
		{
			var r = new Reader<MailMessage>("tompit.mail_dequeue");

			r.CreateParameter("@date", date);
			r.CreateParameter("@next_visible", nextVisible);
			r.CreateParameter("@count", count);

			return r.Execute().ToList<IMailMessage>();
		}

		public void Insert(Guid token, DateTime created, string from, string to, DateTime nextVisible, DateTime expire, string subject, string body, string headers,
			int attachmentCount, MailFormat format)
		{
			var w = new Writer("tompit.mail_queue_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@created", created);
			w.CreateParameter("@from", from);
			w.CreateParameter("@to", to);
			w.CreateParameter("@next_visible", nextVisible);
			w.CreateParameter("@subject", subject);
			w.CreateParameter("@body", body, true);
			w.CreateParameter("@headers", headers, true);
			w.CreateParameter("@attachment_count", attachmentCount);
			w.CreateParameter("@format", format);
			w.Execute();
		}

		public List<IMailMessage> Query()
		{
			return new Reader<MailMessage>("tompit.mail_dequeue").Execute().ToList<IMailMessage>();
		}

		public void Reset(IMailMessage message)
		{
			var w = new Writer("tompit.mail_reset");

			w.CreateParameter("@id", message.GetId());

			w.Execute();
		}

		public IMailMessage Select(Guid token)
		{
			var r = new Reader<MailMessage>("tompit.mail_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public IMailMessage SelectByPopReceipt(Guid popReceipt)
		{
			var r = new Reader<MailMessage>("tompit.mail_sel");

			r.CreateParameter("@pop_receipt", popReceipt);

			return r.ExecuteSingleRow();
		}

		public void Update(Guid popReceipt, string error, DateTime nextVisible)
		{
			var w = new Writer("tompit.mail_upd");

			w.CreateParameter("@pop_receipt", popReceipt);
			w.CreateParameter("@error", error, true);
			w.CreateParameter("@next_visible", nextVisible);

			w.Execute();
		}
	}
}
