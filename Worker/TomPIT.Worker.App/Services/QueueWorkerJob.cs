using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Diagnostics;
using TomPIT.Diagostics;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Storage;
using TomPIT.Worker.Workers;

namespace TomPIT.Worker.Services
{
	public class QueueWorkerJob : DispatcherJob<IQueueMessage>
	{
		private TimeoutTask _timeout = null;
		public QueueWorkerJob(Dispatcher<IQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IQueueMessage item)
		{
			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			_timeout = new TimeoutTask(() =>
			{
				MiddlewareDescriptor.Current.Tenant.Post(MiddlewareDescriptor.Current.Tenant.CreateUrl("QueueManagement", "Ping"), new
				{
					item.PopReceipt
				});

				return Task.CompletedTask;
			}, TimeSpan.FromSeconds(90), Cancel);


			_timeout.Start();

			try
			{
				Invoke(item, m);
			}
			finally
			{
				_timeout.Stop();
				_timeout = null;
			}

			MiddlewareDescriptor.Current.Tenant.Post(MiddlewareDescriptor.Current.Tenant.CreateUrl("QueueManagement", "Complete"), new
			{
				item.PopReceipt
			});
		}

		private void Invoke(IQueueMessage queue, JObject data)
		{
			var component = data.Required<Guid>("component");
			var worker = data.Required<string>("worker");
			var arguments = data.Optional<string>("arguments", null);

			if (!(MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(component) is IQueueConfiguration configuration))
			{
				MiddlewareDescriptor.Current.Tenant.LogError(nameof(Invoke), $"{SR.ErrCannotFindConfiguration} ({component})", nameof(QueueWorkerJob));
				return;
			}

			var w = configuration.Workers.FirstOrDefault(f => string.Compare(f.Name, worker, true) == 0);

			if (w == null)
			{
				MiddlewareDescriptor.Current.Tenant.LogError(nameof(Invoke), $"{SR.ErrQueueWorkerNotFound} ({component})", nameof(QueueWorkerJob));
				return;
			}

			using var ctx = new MicroServiceContext(configuration.MicroService());
			var metricId = ctx.Services.Diagnostic.StartMetric(configuration.Metrics, null);
			Queue q = null;

			try
			{
				q = new Queue(arguments, w);

				q.Invoke();

				ctx.Services.Diagnostic.StopMetric(metricId, Diagnostics.SessionResult.Success, null);
			}
			catch (ValidationException ex)
			{
				if (q.HandlerInstance.ValidationFailed == Cdn.QueueValidationBehavior.Complete)
				{
					MiddlewareDescriptor.Current.Tenant.LogWarning(ex.Source, ex.Message, LogCategories.Worker);
					return;
				}
				else
					throw ex;
			}
			catch
			{
				ctx.Services.Diagnostic.StopMetric(metricId, Diagnostics.SessionResult.Fail, null);

				throw;
			}
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			MiddlewareDescriptor.Current.Tenant.LogError(ex.Source, ex.Message, nameof(QueueWorkerJob));

			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("QueueManagement", "Ping");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt }
			};

			MiddlewareDescriptor.Current.Tenant.Post(url, d);
		}
	}
}
