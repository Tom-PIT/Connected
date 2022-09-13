using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TomPIT.BigData;
using TomPIT.Storage;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers.Management
{
	public class BigDataManagementController : SysController
	{
		/*
		 * Nodes
		 */
		[HttpGet]
		public ImmutableList<INode> QueryNodes()
		{
			return DataModel.BigDataNodes.Query();
		}

		[HttpPost]
		public INode SelectNode()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			return DataModel.BigDataNodes.Select(token);
		}

		[HttpPost]
		public Guid InsertNode()
		{
			var body = FromBody();
			var name = body.Required<string>("name");
			var connectionString = body.Optional("connectionString", string.Empty);
			var adminConnectionString = body.Optional("adminConnectionString", string.Empty);
			var status = body.Required<NodeStatus>("status");

			return DataModel.BigDataNodes.Insert(name, connectionString, adminConnectionString, status);
		}

		[HttpPost]
		public void UpdateNode()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");
			var name = body.Required<string>("name");
			var connectionString = body.Required<string>("connectionString");
			var adminConnectionString = body.Optional("adminConnectionString", string.Empty);
			var status = body.Required<NodeStatus>("status");
			var size = body.Required<long>("size");

			DataModel.BigDataNodes.Update(token, name, connectionString, adminConnectionString, status, size);
		}

		[HttpPost]
		public void DeleteNode()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			DataModel.BigDataNodes.Delete(token);
		}
		/*
		 * Timezones
		 */
		[HttpGet]
		public ImmutableList<ITimeZone> QueryTimeZones()
		{
			return DataModel.BigDataTimeZones.Query();
		}

		[HttpPost]
		public ITimeZone SelectTimeZone()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			return DataModel.BigDataTimeZones.Select(token);
		}

		[HttpPost]
		public Guid InsertTimeZone()
		{
			var body = FromBody();
			var name = body.Required<string>("name");
			var offset = body.Required<int>("offset");

			return DataModel.BigDataTimeZones.Insert(name, offset);
		}

		[HttpPost]
		public void UpdateTimeZone()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");
			var name = body.Required<string>("name");
			var offset = body.Required<int>("offset");

			DataModel.BigDataTimeZones.Update(token, name, offset);
		}

		[HttpPost]
		public void DeleteTimeZone()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			DataModel.BigDataTimeZones.Delete(token);
		}
		/*
		 * Partitions
		 */
		[HttpGet]
		public ImmutableList<IPartition> QueryPartitions()
		{
			return DataModel.BigDataPartitions.Query();
		}

		[HttpPost]
		public IPartition SelectPartition()
		{
			var body = FromBody();
			var configuration = body.Required<Guid>("configuration");

			return DataModel.BigDataPartitions.Select(configuration);
		}

		[HttpPost]
		public void InsertPartition()
		{
			var body = FromBody();
			var name = body.Required<string>("name");
			var configuration = body.Required<Guid>("configuration");
			var status = body.Required<PartitionStatus>("status");
			var resourceGroup = body.Required<Guid>("resourceGroup");

			DataModel.BigDataPartitions.Insert(configuration, name, status, resourceGroup);
		}

		[HttpPost]
		public void UpdatePartition()
		{
			var body = FromBody();
			var configuration = body.Required<Guid>("configuration");
			var name = body.Required<string>("name");
			var status = body.Required<PartitionStatus>("status");

			DataModel.BigDataPartitions.Update(configuration, name, status);
		}

		[HttpPost]
		public void DeletePartition()
		{
			var body = FromBody();
			var configuration = body.Required<Guid>("configuration");

			DataModel.BigDataPartitions.Delete(configuration);
		}
		/*
		 * Transactions
		 */
		[HttpPost]
		public void ActivateTransaction()
		{
			var body = FromBody();
			var transaction = body.Required<Guid>("transaction");

			DataModel.BigDataTransactions.Activate(transaction);
		}

		[HttpPost]
		public Guid InsertTransaction()
		{
			var body = FromBody();
			var partition = body.Required<Guid>("partition");
			var blockCount = body.Required<int>("blockCount");
			var timezone = body.Optional("timezone", Guid.Empty);

			return DataModel.BigDataTransactions.Insert(partition, timezone, blockCount);
		}
		[HttpPost]
		public void DeleteTransaction()
		{
			var body = FromBody();
			var transaction = body.Required<Guid>("transaction");

			DataModel.BigDataTransactions.Delete(transaction);
		}
		/*
		 * Transaction blocks
		 */
		[HttpPost]
		public Guid InsertTransactionBlock()
		{
			var body = FromBody();
			var transaction = body.Required<Guid>("transaction");

			return DataModel.BigDataTransactionBlocks.Insert(transaction);
		}

		[HttpPost]
		public void CompleteTransactionBlock()
		{
			var body = FromBody();
			var popReceipt = body.Required<Guid>("popReceipt");

			DataModel.BigDataTransactionBlocks.Complete(popReceipt);
		}

		[HttpPost]
		public ImmutableList<IQueueMessage> DequeueTransactionBlocks()
		{
			var body = FromBody();
			var count = body.Required<int>("count");
			var nextVisible = body.Required<int>("nextVisible");

			return DataModel.BigDataTransactionBlocks.Dequeue(count, TimeSpan.FromSeconds(nextVisible));
		}

		[HttpPost]
		public void PingTransactionBlock()
		{
			var body = FromBody();
			var popReceipt = body.Required<Guid>("popReceipt");
			var nextVisible = body.Required<int>("nextVisible");

			DataModel.BigDataTransactionBlocks.Ping(popReceipt, TimeSpan.FromSeconds(nextVisible));
		}

		[HttpPost]
		public ITransactionBlock SelectTransactionBlock()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			return DataModel.BigDataTransactionBlocks.Select(token);
		}
		/*
		 * Partition files
		 */
		[HttpPost]
		public Guid InsertFile()
		{
			var body = FromBody();
			var partition = body.Required<Guid>("partition");
			var node = body.Required<Guid>("node");
			var key = body.Optional("key", string.Empty);
			var timestamp = body.Optional("timeStamp", DateTime.MinValue);
			var timezone = body.Optional("timezone", Guid.Empty);

			return DataModel.BigDataPartitionFiles.Insert(partition, node, timezone, key, timestamp);
		}

		[HttpPost]
		public void UpdateFile()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");
			var startTimestamp = body.Optional("startTimeStamp", DateTime.MinValue);
			var endTimestamp = body.Optional("endTimeStamp", DateTime.MinValue);
			var count = body.Required<int>("count");
			var status = body.Required<PartitionFileStatus>("status");

			DataModel.BigDataPartitionFiles.Update(token, startTimestamp, endTimestamp, count, status);
		}

		[HttpGet]
		public ImmutableList<IPartitionFile> QueryFiles()
		{
			return DataModel.BigDataPartitionFiles.Query();
		}

		[HttpPost]
		public ImmutableList<IPartitionFile> QueryFilesForPartition()
		{
			var body = FromBody();
			var partition = body.Required<Guid>("partition");

			return DataModel.BigDataPartitionFiles.Query(partition);
		}

		[HttpPost]
		public IPartitionFile SelectFile()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			return DataModel.BigDataPartitionFiles.Select(token);
		}

		[HttpPost]
		public Guid LockFile()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			return DataModel.BigDataPartitionFiles.Lock(token);
		}

		[HttpPost]
		public void UnlockFile()
		{
			var body = FromBody();
			var key = body.Required<Guid>("unlockKey");

			DataModel.BigDataPartitionFiles.Unlock(key);
		}

		[HttpPost]
		public void DeleteFile()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			DataModel.BigDataPartitionFiles.Delete(token);
		}
		/*
  		 * Field statistics
		 */
		[HttpGet]
		public ImmutableList<IPartitionFieldStatistics> QueryFieldStatistics()
		{
			return DataModel.BigDataPartitionFieldStatistics.Query();
		}
		[HttpPost]
		public IPartitionFieldStatistics SelectFieldStatistic()
		{
			var body = FromBody();
			var file = body.Required<Guid>("file");
			var fieldName = body.Required<string>("fieldName");

			return DataModel.BigDataPartitionFieldStatistics.Select(file, fieldName);
		}
		[HttpPost]
		public void UpdateFieldStatistics()
		{
			var body = FromBody();
			var file = body.Required<Guid>("file");
			var fieldName = body.Required<string>("fieldName");
			var startString = body.Optional("startString", string.Empty);
			var endString = body.Optional("endString", string.Empty);
			var startNumber = body.Optional("startNumber", 0d);
			var endNumber = body.Optional("endNumber", 0d);
			var startDate = body.Optional("startDate", DateTime.MinValue);
			var endDate = body.Optional("endDate", DateTime.MinValue);

			DataModel.BigDataPartitionFieldStatistics.Update(file, fieldName, startString, endString, startNumber, endNumber, startDate, endDate);
		}
		/*
		 * Maintenance
		 */
		[HttpPost]
		public ImmutableList<IQueueMessage> DequeueMaintenance()
		{
			var body = FromBody();
			var count = body.Required<int>("count");
			var nextVisible = body.Required<int>("nextVisible");

			return DataModel.BigDataPartitions.DequeueMaintenance(count, TimeSpan.FromSeconds(nextVisible));
		}

		[HttpPost]
		public void PingMaintenance()
		{
			var body = FromBody();
			var popReceipt = body.Required<Guid>("popReceipt");
			var nextVisible = body.Required<int>("nextVisible");

			DataModel.BigDataPartitions.Ping(popReceipt, TimeSpan.FromSeconds(nextVisible));
		}

		[HttpPost]
		public void CompleteMaintenance()
		{
			var body = FromBody();
			var popReceipt = body.Required<Guid>("popReceipt");

			DataModel.BigDataPartitions.Complete(popReceipt);
		}
		/*
		 * Buffering
		 */
		[HttpPost]
		public List<IPartitionBuffer> DequeueBuffers()
		{
			var body = FromBody();
			var count = body.Required<int>("count");
			var ts = body.Required<TimeSpan>("timeSpan");

			return DataModel.BigDataPartitionBuffering.Dequeue(count, ts).ToList<IPartitionBuffer>();
		}
		[HttpPost]
		public void EnqueueBuffer()
		{
			var body = FromBody();
			var partition = body.Required<Guid>("partition");
			var duration = body.Required<TimeSpan>("duration");
			var data = body.Required<string>("data");

			DataModel.BigDataPartitionBuffering.Enqueue(partition, duration, Convert.FromBase64String(data));
		}
		[HttpPost]
		public List<IPartitionBufferData> QueryBufferData()
		{
			var body = FromBody();
			var partition = body.Required<Guid>("partition");

			return DataModel.BigDataPartitionBuffering.QueryData(partition);
		}
		[HttpPost]
		public void ClearBufferData()
		{
			var body = FromBody();
			var partition = body.Required<Guid>("partition");
			var nextVisible = body.Required<TimeSpan>("nextVisible");
			var id = body.Required<long>("id");

			DataModel.BigDataPartitionBuffering.Clear(partition, nextVisible, id);
		}
	}
}
