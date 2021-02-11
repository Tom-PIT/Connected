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

		public override DispatcherJob<IMailMessage> CreateWorker(IDispatcher<IMailMessage> owner, CancellationToken cancel)
		{
			return new MailJob(owner, cancel);
		}

		public string ResourceGroup { get; }
	}
}
