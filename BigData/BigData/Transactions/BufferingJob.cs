﻿using LZ4;

using Newtonsoft.Json.Linq;

using System;
using System.Linq;
using System.Text;
using System.Threading;

using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.BigData.Transactions
{
	internal class BufferingJob : DispatcherJob<IPartitionBuffer>
	{
		public BufferingJob(IDispatcher<IPartitionBuffer> owner, CancellationToken cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IPartitionBuffer item)
		{
			var config = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(item.Partition) as IPartitionConfiguration;
			//TODO: if no config we should probably need to delete the buffer permanently

			if (config == null)
			{
				Complete(item.Partition, TimeSpan.FromMinutes(10), 0);
				return;
			}

			using var ctx = new MicroServiceContext(config.MicroService());
			var middlewareType = config.Middleware(ctx);

			if (middlewareType == null)
			{
				Complete(item.Partition, TimeSpan.FromMinutes(1), 0);
				return;
			}

			var middleware = ctx.CreateMiddleware<IPartitionComponent>(middlewareType);

			if (middleware == null)
			{
				Complete(item.Partition, TimeSpan.FromMinutes(1), 0);
				return;
			}
			//TODO check this out
			var items = MiddlewareDescriptor.Current.Tenant.GetService<IBufferingService>().QueryData(item.Partition);

			foreach (var i in items)
			{
				var data = Serializer.Deserialize<JArray>(Encoding.UTF8.GetString(LZ4Codec.Unwrap(i.Data)));

				if (data.Any())
				{
					try
					{
						MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().CreateTransactions(config, data);
						data.Clear();
					}
					catch (Exception ex)
					{
						ctx.Services.Diagnostic.Error(nameof(BufferingJob), ex.ToString(), LogCategories.BigData);
					}
				}
				
				GC.Collect();
			}

			Complete(item.Partition, middleware.BufferTimeout, items == null || items.Count == 0 ? 0 : items.Max(f => f.Id));
		}

		private void Complete(Guid partition, TimeSpan nextVisible, long id)
		{
			MiddlewareDescriptor.Current.Tenant.GetService<IBufferingService>().ClearData(partition, nextVisible, id);
		}

		protected override void OnError(IPartitionBuffer item, Exception ex)
		{
			Complete(item.Partition, TimeSpan.FromSeconds(30), 0);
		}
	}
}