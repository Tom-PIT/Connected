﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;

namespace TomPIT.BigData.Transactions
{
	internal class BufferingWorker : HostedService
	{
		private Lazy<List<BufferingDispatcher>> _dispatchers = new Lazy<List<BufferingDispatcher>>();

		public BufferingWorker()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);
		}

		protected override bool Initialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initialining)
				return false;

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new BufferingDispatcher(i, cancel));

			return true;
		}

		protected override Task Process(CancellationToken cancel)
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var jobs = MiddlewareDescriptor.Current.Tenant.GetService<IBufferingService>().Dequeue(f.Available);

				if (cancel.IsCancellationRequested)
					return;

				if (jobs == null)
					return;

				foreach (var i in jobs)
				{
					if (cancel.IsCancellationRequested)
						return;

					f.Enqueue(i);
				}
			});

			return Task.CompletedTask;
		}

		private List<BufferingDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}