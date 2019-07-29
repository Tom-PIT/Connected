using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Connectivity;
using TomPIT.Environment;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.BigData.Services
{
	internal class TransactionService : ServiceBase, ITransactionService
	{
		public TransactionService(ISysConnection connection) : base(connection)
		{
		}

		public void Complete(Guid popReceipt, Guid block)
		{
			var transactionBlock = Select(block);
				
			var u = Connection.CreateUrl("BigDataManagement", "CompleteTransactionBlock");
			var e = new JObject
			{
				{"popReceipt", popReceipt }
			};

			Connection.Post(u, e);

			if (transactionBlock == null)
				return;

			var configuration = Connection.GetService<IComponentService>().SelectConfiguration(transactionBlock.Partition) as IPartitionConfiguration;
			var partition = Connection.GetService<IPartitionService>().Select(configuration);
			var microService = Connection.GetService<IMicroServiceService>().Select(((IConfiguration)configuration).MicroService(Connection));
			var blobs = Connection.GetService<IStorageService>().Query(microService.Token, BlobTypes.BigDataTransactionBlock, microService.ResourceGroup, block.ToString());

			foreach (var blob in blobs)
				Connection.GetService<IStorageService>().Delete(blob.Token);
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			var u = Connection.CreateUrl("BigDataManagement", "DequeueTransactionBlocks");
			var e = new JObject
			{
				{"count", count },
				{"nextVisible", 60 }
			};

			return Connection.Post<List<QueueMessage>>(u, e).ToList<IQueueMessage>();
		}

		public void Ping(Guid popReceipt)
		{
			var u = Connection.CreateUrl("BigDataManagement", "PingTransactionBlock");
			var e = new JObject
			{
				{"popReceipt", popReceipt },
				{"nextVisible", 60 }
			};

			Connection.Post(u, e);
		}

		public ITransactionBlock Select(Guid token)
		{
			var u = Connection.CreateUrl("BigDataManagement", "SelectTransactionBlock");
			var e = new JObject
			{
				{"token", token }
			};

			return Connection.Post<TransactionBlock>(u, e);
		}

		public void Prepare(IPartitionConfiguration partition, JArray items)
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
			var partition = Connection.GetService<IPartitionService>().Select(configuration);
			var u = Connection.CreateUrl("BigDataManagement", "InsertTransaction");
			var e = new JObject
			{
				{"partition", partition.Configuration},
				{"blockCount", blockCount }
			};

			return Connection.Post<Guid>(u, e);
		}

		private Guid InsertBlock(Guid microService, Guid transaction, List<object> items)
		{
			var u = Connection.CreateUrl("BigDataManagement", "InsertTransactionBlock");
			var e = new JObject
					{
						{"transaction", transaction }
					};

			var blockId = Connection.Post<Guid>(u, e);

			Connection.GetService<IStorageService>().Upload(new TransactionBlockBlob
			{
				ContentType = "application/json",
				FileName = $"{blockId.ToString()}.json",
				MicroService = microService,
				PrimaryKey = blockId.ToString(),
				Token = blockId,
				ResourceGroup = Connection.GetService<IResourceGroupService>().Default.Token,
				Type = BlobTypes.BigDataTransactionBlock
			}, Encoding.UTF8.GetBytes(Types.Serialize(items)), StoragePolicy.Singleton);

			return blockId;
		}

		private void ActivateTransaction(Guid transaction)
		{
			var u = Connection.CreateUrl("BigDataManagement", "ActivateTransaction");
			var e = new JObject
			{
				{"transaction", transaction}
			};

			Connection.Post(u, e);
		}

		private void DeleteTransaction(Guid transaction, List<Guid> blobs)
		{
			var u = Connection.CreateUrl("BigDataManagement", "DeleteTransaction");
			var e = new JObject
			{
				{"transaction", transaction }
			};

			Connection.Post(u, e);

			foreach (var blob in blobs)
				Connection.GetService<IStorageService>().Delete(blob);
		}
	}
}
