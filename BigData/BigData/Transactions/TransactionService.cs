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
		public TransactionService(ITenant tenant) : base(tenant)
		{
		}

		public void Complete(Guid popReceipt, Guid block)
		{
			var transactionBlock = Select(block);

			var u = Tenant.CreateUrl("BigDataManagement", "CompleteTransactionBlock");
			var e = new JObject
			{
				{"popReceipt", popReceipt }
			};

			Tenant.Post(u, e);

			if (transactionBlock == null)
				return;

			var configuration = Tenant.GetService<IComponentService>().SelectConfiguration(transactionBlock.Partition) as IPartitionConfiguration;
			var partition = Tenant.GetService<IPartitionService>().Select(configuration);
			var microService = Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)configuration).MicroService());
			var blobs = Tenant.GetService<IStorageService>().Query(microService.Token, BlobTypes.BigDataTransactionBlock, microService.ResourceGroup, block.ToString());

			foreach (var blob in blobs)
				Tenant.GetService<IStorageService>().Delete(blob.Token);
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			if (count == 0)
				return new List<IQueueMessage>();

			var u = Tenant.CreateUrl("BigDataManagement", "DequeueTransactionBlocks");
			var e = new JObject
			{
				{"count", count },
				{"nextVisible", 60 }
			};

			return Tenant.Post<List<QueueMessage>>(u, e).ToList<IQueueMessage>();
		}

		public void Ping(Guid popReceipt, TimeSpan delay)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "PingTransactionBlock");
			var e = new JObject
			{
				{"popReceipt", popReceipt },
				{"nextVisible", Convert.ToInt32(delay.TotalSeconds) }
			};

			Tenant.Post(u, e);
		}

		public ITransactionBlock Select(Guid token)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "SelectTransactionBlock");
			var e = new JObject
			{
				{"token", token }
			};

			return Tenant.Post<TransactionBlock>(u, e);
		}

		public void Prepare(IPartitionConfiguration partition, JArray items)
		{
			using var ctx = new MicroServiceContext(partition.MicroService(), Tenant.Url);
			var middleware = partition.Middleware(ctx);

			if (middleware == null)
				CreateTransaction(partition, items);
			else
			{
				var instance = ctx.CreateMiddleware<IPartitionComponent>(middleware);

				if (instance == null || !instance.Buffered)
					CreateTransaction(partition, items);
				else
					BufferItems(instance, partition, items);
			}
		}

		private void BufferItems(IPartitionComponent middleware, IPartitionConfiguration configuration, JArray items)
		{
			Tenant.GetService<IBufferingService>().Enqueue(configuration.Component, middleware.BufferTimeout, items);
		}

		public void CreateTransaction(IPartitionConfiguration partition, JArray items)
		{
			var parser = new TransactionParser(partition, items);

			parser.Execute();

			var transactionId = InsertTransaction(partition, parser.BlockCount);
			var blobs = new List<Guid>();

			try
			{
				foreach (var block in parser.Blocks)
					blobs.Add(InsertBlock(parser.MicroService.Token, transactionId, block));

				ActivateTransaction(transactionId);
			}
			catch
			{
				DeleteTransaction(transactionId, blobs);

				throw;
			}
		}

		private Guid InsertTransaction(IPartitionConfiguration configuration, int blockCount)
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
	}
}
