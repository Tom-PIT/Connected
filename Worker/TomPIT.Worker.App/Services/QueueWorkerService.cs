using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Serialization;
using TomPIT.Sys.Model;

namespace TomPIT.Worker.Services;

internal class QueueWorkerService : HostedService
{
	private QueueWorkerDispatcher _dispatcher = new();

	private readonly IQueueMonitoringService _queueMonitoringService;

	public static QueueWorkerService ServiceInstance { get; private set; }

	public QueueWorkerService()
	{
		IntervalTimeout = TimeSpan.FromMilliseconds(new DispatcherConfig().QueueDequeueInterval);
		_queueMonitoringService = Tenant.GetService<IQueueMonitoringService>();
		ServiceInstance = this;
	}

	protected override bool OnInitialize(CancellationToken cancel)
	{
		if (Instance.State == InstanceState.Initializing)
			return false;

		return true;
	}
	protected override async Task OnExecute(CancellationToken cancel)
	{
		if (_dispatcher.Available < 1)
			return;

		var jobs = DataModel.Workers.Dequeue(_dispatcher.Available);

		_queueMonitoringService?.SignalEnqueued(jobs?.Count ?? 0);

		var batch = Guid.NewGuid();

		if (cancel.IsCancellationRequested)
			return;

		if (jobs is null)
			return;

		foreach (var i in jobs)
		{
			if (cancel.IsCancellationRequested)
				return;

			MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Dump($"{typeof(QueueWorkerService).FullName.PadRight(64)}| Batch {batch} => Enqueue {Serializer.Serialize(i)}");

			_dispatcher.Enqueue(i);
		}

		await Task.CompletedTask;
	}

	public override void Dispose()
	{
		_dispatcher.Dispose();

		base.Dispose();
	}
}
