using System.Threading;
using TomPIT.Distributed;

namespace TomPIT.Cdn.Mail
{
	internal class MailDispatcher : Dispatcher<IMailMessage>
	{
		public MailDispatcher(string resourceGroup, CancellationToken cancel) : base(cancel, 128)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IMailMessage> CreateWorker(CancellationToken cancel)
		{
			return new MailJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
