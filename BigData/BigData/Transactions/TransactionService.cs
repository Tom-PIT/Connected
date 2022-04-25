using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.BigData.Partitions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Connectivity;
using TomPIT.Distributed;
using TomPIT.Environment;
using TomPIT.Middleware;
using TomPIT.Serialization;
using TomPIT.Storage;

namespace TomPIT.BigData.Transactions
{
	internal class TransactionService : TenantObject, ITransactionService
	{
		private const string DefaultController = "BigDataManagement";
		public TransactionService(ITenant tenant) : base(tenant)
		{
		}

		public void Complete(Guid popReceipt, Guid block)
		{
			var transactionBlock = Select(block);

			Tenant.Post(CreateUrl("CompleteTransactionBlock"), new { popReceipt });

			if (transactionBlock is null)
				return;

			var configuration = Tenant.GetService<IComponentService>().SelectConfiguration(transactionBlock.Partition) as IPartitionConfiguration;
			var microService = Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)configuration).MicroService());
			var blobs = Tenant.GetService<IStorageService>().Query(microService.Token, BlobTypes.BigDataTransactionBlock, microService.ResourceGroup, block.ToString());

			foreach (var blob in blobs)
				Tenant.GetService<IStorageService>().Delete(blob.Token);
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			if (count == 0)
				return new List<IQueueMessage>();

			return Tenant.Post<List<QueueMessage>>(CreateUrl("DequeueTransactionBlocks"), new
			{
				count,
				nextVisible = 600
			}).ToList<IQueueMessage>();
		}

		public void Ping(Guid popReceipt, TimeSpan delay)
		{
			Tenant.Post(CreateUrl("PingTransactionBlock"), new
			{
				popReceipt,
				nextVisible = Convert.ToInt32(delay.TotalSeconds)
			});
		}

		public ITransactionBlock Select(Guid token)
		{
			return Tenant.Post<TransactionBlock>(CreateUrl("SelectTransactionBlock"), new { token });
		}

		public void Prepare(IPartitionConfiguration partition, JArray items)
		{
			using var ctx = new MicroServiceContext(partition.MicroService(), Tenant.Url);
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
			var parser = new TransactionParser(partition, items);

			parser.Execute();
			
			var supportsTimezone = partition.SupportsTimezone();
			var timezones = supportsTimezone ? Tenant.GetService<ITimezoneService>().Query() : null;
			var blobs = new Dictionary<Guid, List<Guid>>();

			try
			{
				CreateTransaction(partition, null, parser, blobs);

				if(supportsTimezone)
				{
					foreach (var timezone in timezones)
						CreateTransaction(partition, timezone, parser, blobs);
				}

				foreach (var transaction in blobs)
					ActivateTransaction(transaction.Key);
			}
			catch
			{
				foreach (var transaction in blobs)
					DeleteTransaction(transaction.Key, transaction.Value);
			}
		}

		private void CreateTransaction(IPartitionConfiguration partition, ITimezone timezone, TransactionParser parser, Dictionary<Guid, List<Guid>> transactions)
		{
			var transactionId = InsertTransaction(partition, timezone, parser.BlockCount);
			var blocks = new List<Guid>();

			transactions.Add(transactionId, blocks);

			foreach (var block in parser.Blocks)
				blocks.Add(InsertBlock(parser.MicroService.Token, transactionId, block));
		}

		private Guid InsertTransaction(IPartitionConfiguration configuration, ITimezone timezone, int blockCount)
		{
			var partition = Tenant.GetService<IPartitionService>().Select(configuration);
			var u = Tenant.CreateUrl("BigDataManagement", "InsertTransaction");
			var e = new JObject
			{
				{"partition", partition.Configuration},
				{"blockCount", blockCount }
			};

			return Tenant.Post<Guid>(u, e);
		}

		private Guid InsertBlock(Guid microService, Guid transaction, List<object> items)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "InsertTransactionBlock");
			var e = new JObject
					{
						{"transaction", transaction }
					};

			var blockId = Tenant.Post<Guid>(u, e);

			Tenant.GetService<IStorageService>().Upload(new TransactionBlockBlob
			{
				ContentType = "application/json",
				FileName = $"{blockId.ToString()}.json",
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
			var u = Tenant.CreateUrl("BigDataManagement", "ActivateTransaction");
			var e = new JObject
			{
				{"transaction", transaction}
			};

			Tenant.Post(u, e);
		}

		private void DeleteTransaction(Guid transaction, List<Guid> blobs)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "DeleteTransaction");
			var e = new JObject
			{
				{"transaction", transaction }
			};

			Tenant.Post(u, e);

			foreach (var blob in blobs)
				Tenant.GetService<IStorageService>().Delete(blob);
		}

		private string CreateUrl(string action)
		{
			return Tenant.CreateUrl(DefaultController, action);
		}
	}
}
