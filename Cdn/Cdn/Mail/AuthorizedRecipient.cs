using MimeKit;

namespace TomPIT.Cdn.Mail
{
	internal enum RecipientKind
	{
		Bounce = 1,
		Abuse = 2,
		Content = 3
	}

	internal class AuthorizedRecipient
	{
		public MailboxAddress Email { get; set; }
		public RecipientKind Kind { get; set; }
	}
}
