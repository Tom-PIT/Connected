using TomPIT.Connectivity;

namespace TomPIT.Cdn.Mail
{
	internal interface IInboxService : ITenantObject
	{
		InboxMessageResult ProcessMail(string recipientEmail, IInboxMessage message);
	}
}
