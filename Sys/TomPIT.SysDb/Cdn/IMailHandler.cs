using System;
using System.Collections.Generic;
using TomPIT.Cdn;

namespace TomPIT.SysDb.Cdn
{
	public interface IMailHandler
	{
		void Clear();
		void Delete(IMailMessage message);
		void Delete(Guid popReceipt);
		List<IMailMessage> Dequeue(DateTime date, DateTime nextVisible, int count);
		List<IMailMessage> Query();
		void Reset(IMailMessage message);
		IMailMessage Select(Guid token);
		IMailMessage SelectByPopReceipt(Guid popReceipt);

		void Insert(Guid token, DateTime created, string from, string to, DateTime nextVisible, DateTime expire, string subject, string body, string headers, int attachmentCount, MailFormat format);
		void Update(Guid popReceipt, string error, DateTime nextVisible);
	}
}
