using TomPIT.SysDb.Cdn;

namespace TomPIT.SysDb.Sql.Cdn
{
	internal class CdnHandler : ICdnHandler
	{
		private IMailHandler _mail = null;
		private ISubscriptionHandler _subscription = null;

		public IMailHandler Mail
		{
			get
			{
				if (_mail == null)
					_mail = new MailHandler();

				return _mail;
			}
		}

		public ISubscriptionHandler Subscription
		{
			get
			{
				if (_subscription == null)
					_subscription = new SubscriptionHandler();

				return _subscription;
			}
		}
	}
}
