using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Events
{
	internal class EventJob : DispatcherJob<IEventQueueMessage>
	{
		public EventJob(IDispatcher<IEventQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IEventQueueMessage item)
		{
			var timeout = new TimeoutTask(() =>
			{
				Delay(item.PopReceipt, 300);

				return Task.CompletedTask;
			}, TimeSpan.FromMinutes(4), Cancel);


			timeout.Start();

			try
			{
				if (!Invoke(item))
					return;
			}
			finally
			{
				timeout.Stop();
				timeout = null;
			}

			Instance.SysProxy.Management.Events.Complete(item.PopReceipt);
		}

		private static void Delay(Guid popReceipt, int delay)
		{
			Instance.SysProxy.Management.Events.Ping(popReceipt, delay);
		}

		private bool Invoke(IEventQueueMessage message)
		{
			using var executor = new EventExecutor(Owner, message);

			return executor.Invoke();
		}

		protected override void OnError(IEventQueueMessage message, Exception ex)
		{
			if (ex is MiddlewareValidationException mw)
				mw.LogWarning(LogCategories.Cdn);

			TomPITException.Unwrap(this, ex).LogError(LogCategories.Cdn);

			Instance.SysProxy.Management.Events.Complete(message.PopReceipt);
		}
	}
}
