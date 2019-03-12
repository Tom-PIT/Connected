using System.Threading;
using TomPIT.Services;

namespace TomPIT.Cdn.Services
{
	internal class MailDispatcher : Dispatcher<IMailMessage>
	{
		public MailDispatcher(string resourceGroup, CancellationTokenSource cancel) : base(cancel, 128)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IMailMessage> CreateWorker(CancellationTokenSource cancel)
		{
			return new MailJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
