using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TomPIT.BigData.Partitions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Diagnostics.Tracing;
using TomPIT.Environment;
using TomPIT.Middleware;
using TomPIT.Serialization;
using TomPIT.Storage;

namespace TomPIT.BigData.Transactions
{
	internal class TransactionService : TenantObject, ITransactionService
	{
		private readonly ITraceService _traceService;
		private readonly ITraceEndpoint _createTransactionEndpoint = new TraceEndpoint("BigData.Transactions", "CreateTransaction");

		public TransactionService(ITenant tenant) : base(tenant)
		{
			_traceService = MiddlewareDescriptor.Current.Tenant.GetService<ITraceService>();
			_traceService?.AddEndpoint(_createTransactionEndpoint);
		}

		public void Complete(Guid popReceipt, Guid block)
		{
			try
			{
				var transactionBlock = Select(block);

				if (transactionBlock is null)
					return;

				var configuration = Tenant.GetService<IComponentService>().SelectConfiguration(transactionBlock.Partition) as IPartitionConfiguration;
				var microService = Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)configuration).MicroService());
				var blobs = Tenant.GetService<IStorageService>().Query(microService.Token, BlobTypes.BigDataTransactionBlock, microService.ResourceGroup, block.ToString());

				foreach (var blob in blobs)
					Tenant.GetService<IStorageService>().Delete(blob.Token);
			}
			finally
			{
				Instance.SysProxy.Management.BigData.CompleteTransactionBlock(popReceipt);
			}
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			if (count == 0)
				return new List<IQueueMessage>();

			return Instance.SysProxy.Management.BigData.DequeueTransactionBlocks(count, 600).ToList();
		}

		public void Ping(Guid popReceipt, TimeSpan delay)
		{
			Instance.SysProxy.Management.BigData.PingTransactionBlock(popReceipt, Convert.ToInt32(delay.TotalSeconds));
		}

		public ITransactionBlock Select(Guid token)
		{
			return Instance.SysProxy.Management.BigData.SelectTransactionBlock(token);
		}

		public void Prepare(IPartitionConfiguration partition, JArray items)
		{
			using var ctx = new MicroServiceContext(partition.MicroService());
			var middleware = partition.Middleware(ctx);

			if (middleware is null)
				CreateTransactions(partition, items);
			else
			{
				var instance = ctx.CreateMiddleware<IPartitionComponent>(middleware);

				if (instance is null || !instance.Buffered)
					CreateTransactions(partition, items);
				else
					BufferItems(instance, partition, items);
			}
		}

		private void BufferItems(IPartitionComponent middleware, IPartitionConfiguration configuration, JArray items)
		{
			Tenant.GetService<IBufferingService>().Enqueue(configuration.Component, middleware.BufferTimeout, items);
		}

		public void CreateTransactions(IPartitionConfiguration partition, JArray items)
		{
			using var parser = new TransactionParser(partition, items);

			parser.Execute();

			var supportsTimezone = partition.SupportsTimezone();
			var timezones = supportsTimezone ? Tenant.GetService<ITimeZoneService>().Query() : null;
			var blobs = new Dictionary<Guid, List<Guid>>();

			//_traceService?.Trace(new TraceMessage(_createTransactionEndpoint, Serializer.Serialize(new
			//{
			//	Partition = partition.FileName,
			//	ItemCount = items?.Count,
			//	SupportsTimezone = supportsTimezone,
			//	Timezones = timezones?.Select(e => e.Name)
			//})));

			try
			{
				CreateTransaction(partition, null, parser, blobs);

				if (supportsTimezone)
				{
					foreach (var timezone in timezones)
						CreateTransaction(partition, timezone, parser, blobs);
				}

				foreach (var transaction in blobs)
					ActivateTransaction(transaction.Key);
			}
			catch (Exception ex)
			{
				try
				{
					Tenant.LogError($"Big data - {nameof(TransactionService)}/{nameof(CreateTransactions)}", ex.ToString());
				}
				finally
				{
					foreach (var transaction in blobs)
						DeleteTransaction(transaction.Key, transaction.Value);
				}
			}
		}

		private void CreateTransaction(IPartitionConfiguration partition, ITimeZone timezone, TransactionParser parser, Dictionary<Guid, List<Guid>> transactions)
		{
			var transactionId = InsertTransaction(partition, timezone, parser.BlockCount);
			var blocks = new List<Guid>();

			transactions.Add(transactionId, blocks);

			foreach (var block in parser.Blocks)
				blocks.Add(InsertBlock(parser.MicroService.Token, transactionId, block));
		}

		private Guid InsertTransaction(IPartitionConfiguration configuration, ITimeZone timezone, int blockCount)
		{
			var partition = Tenant.GetService<IPartitionService>().Select(configuration);

			return Instance.SysProxy.Management.BigData.InsertTransaction(partition.Configuration, blockCount, timezone is null ? Guid.Empty : timezone.Token);
		}

		private Guid InsertBlock(Guid microService, Guid transaction, List<object> items)
		{
			var blockId = Instance.SysProxy.Management.BigData.InsertTransactionBlock(transaction);

			Tenant.GetService<IStorageService>().Upload(new TransactionBlockBlob
			{
				ContentType = "application/json",
				FileName = $"{blockId}.json",
				MicroService = microService,
				PrimaryKey = blockId.ToString(),
				Token = blockId,
				ResourceGroup = Tenant.GetService<IResourceGroupService>().Default.Token,
				Type = BlobTypes.BigDataTransactionBlock
			}, Encoding.UTF8.GetBytes(Serializer.Serialize(items)), StoragePolicy.Singleton);

			return blockId;
		}

		private void ActivateTransaction(Guid transaction)
		{
			Instance.SysProxy.Management.BigData.ActivateTransaction(transaction);
		}

		private void DeleteTransaction(Guid transaction, List<Guid> blobs)
		{
			Instance.SysProxy.Management.BigData.DeleteTransaction(transaction);

			foreach (var blob in blobs)
				Tenant.GetService<IStorageService>().Delete(blob);
		}
	}
}
