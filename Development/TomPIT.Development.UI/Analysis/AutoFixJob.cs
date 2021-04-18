using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design.Tools;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Ide.Designers;
using TomPIT.Middleware;
using TomPIT.Serialization;
using TomPIT.Storage;

namespace TomPIT.Development.Analysis
{
	internal class AutoFixJob : DispatcherJob<IQueueMessage>
	{
		private TimeoutTask _timeout = null;
		public AutoFixJob(IDispatcher<IQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
		{
		}

		private AutoFixDispatcher Dispatcher => Owner as AutoFixDispatcher;
		private IQueueMessage Message { get; set; }

		protected override void DoWork(IQueueMessage item)
		{
			Message = item;

			var m = Serializer.Deserialize<JObject>(item.Message);

			_timeout = new TimeoutTask(() =>
			{
				Dispatcher.Tenant.GetService<IAutoFixService>().Ping(Message.PopReceipt);
				return Task.CompletedTask;
			}, TimeSpan.FromMinutes(4), Cancel);

			_timeout.Start();

			try
			{
				Invoke(item, m);
			}
			finally
			{
				_timeout.Stop();
			}
		}

		private void Invoke(IQueueMessage queue, JObject data)
		{
			var errorId = data.Required<Guid>("error");
			var provider = data.Required<string>("provider");
			var error = Dispatcher.Tenant.GetService<IDesignerService>().SelectError(errorId);

			var providers = Dispatcher.Tenant.GetService<IDesignerService>().QueryAutoFixProviders();
			var fixProvider = providers.FirstOrDefault(f => string.Compare(f.Name, provider, true) == 0);

			if (fixProvider != null)
			{
				var ms = Dispatcher.Tenant.GetService<IMicroServiceService>().Select(error.MicroService);
				using var ctx = new MicroServiceContext(ms, Dispatcher.Tenant.Url);
				fixProvider.Fix(this, new AutoFixArgs(ctx, error));
			}

			Dispatcher.Tenant.GetService<IAutoFixService>().Complete(queue.PopReceipt);
		}


		protected override void OnError(IQueueMessage item, Exception ex)
		{
			Dispatcher.Tenant.LogError(ex.Source, ex.Message, nameof(AutoFixJob));
			Dispatcher.Tenant.GetService<IAutoFixService>().Complete(item.PopReceipt);
		}
	}
}
