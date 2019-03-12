using MimeKit;

namespace TomPIT.Cdn
{
	internal static class CdnExtensions
	{
		public static string ReceiverDomain(this IMailMessage message)
		{
			if (!MailboxAddress.TryParse(message.To, out MailboxAddress address))
				return null;

			var emailTokens = address.Address.Trim().Split('@');

			if (emailTokens.Length != 2)
				return null;

			return emailTokens[1];
		}
	}
}
