using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.BigData;
using TomPIT.Distributed;
using TomPIT.Proxy.Management;
using TomPIT.Storage;

namespace TomPIT.Proxy.Remote.Management
{
	internal class BigDataManagementController : IBigDataManagementController
	{
		private const string Controller = "BigDataManagement";

		public void ActivateTransaction(Guid transaction)
		{
			Connection.Post(Connection.CreateUrl(Controller, "ActivateTransaction"), new
			{
				transaction
			});
		}

		public void ClearBufferData(Guid partition, TimeSpan nextVisible, long id)
		{
			Connection.Post(Connection.CreateUrl(Controller, "ClearBufferData"), new
			{
				partition,
				nextVisible,
				id
			});
		}

		public void CompleteMaintenance(Guid popReceipt)
		{
			Connection.Post(Connection.CreateUrl(Controller, "CompleteMaintenance"), new
			{
				popReceipt
			});
		}

		public void CompleteTransactionBlock(Guid popReceipt)
		{
			Connection.Post(Connection.CreateUrl(Controller, "CompleteTransactionBlock"), new
			{
				popReceipt
			});
		}

		public void DeleteFile(Guid token)
		{
			Connection.Post(Connection.CreateUrl(Controller, "DeleteFile"), new
			{
				token
			});
		}

		public void DeleteNode(Guid token)
		{
			Connection.Post(Connection.CreateUrl(Controller, "DeleteNode"), new
			{
				token
			});
		}

		public void DeletePartition(Guid configuration)
		{
			Connection.Post(Connection.CreateUrl(Controller, "DeletePartition"), new
			{
				configuration
			});
		}

		public void DeleteTimeZone(Guid token)
		{
			Connection.Post(Connection.CreateUrl(Controller, "DeleteTimeZone"), new
			{
				token
			});
		}

		public void DeleteTransaction(Guid transaction)
		{
			Connection.Post(Connection.CreateUrl(Controller, "DeleteTransaction"), new
			{
				transaction
			});
		}

		public ImmutableList<IPartitionBuffer> DequeueBuffers(int count, TimeSpan timespan)
		{
			return Connection.Post<List<PartitionBuffer>>(Connection.CreateUrl(Controller, "DequeueBuffers"), new
			{
				count,
				timespan
			}).ToImmutableList<IPartitionBuffer>();
		}

		public ImmutableList<IQueueMessage> DequeueMaintenance(int count, int nextVisible)
		{
			return Connection.Post<List<QueueMessage>>(Connection.CreateUrl(Controller, "DequeueMaintenance"), new
			{
				count,
				nextVisible
			}).ToImmutableList<IQueueMessage>();
		}

		public ImmutableList<IQueueMessage> DequeueTransactionBlocks(int count, int nextVisible)
		{
			return Connection.Post<List<QueueMessage>>(Connection.CreateUrl(Controller, "DequeueTransactionBlocks"), new
			{
				count,
				nextVisible
			}).ToImmutableList<IQueueMessage>();
		}

		public void EnqueueBuffer(Guid partition, TimeSpan duration, byte[] data)
		{
			Connection.Post(Connection.CreateUrl(Controller, "EnqueueBuffer"), new
			{
				partition,
				duration,
				data
			});
		}

		public Guid InsertFile(Guid partition, Guid node, string key, DateTime timestamp, Guid timezone)
		{
			return Connection.Post<Guid>(Connection.CreateUrl(Controller, "InsertFile"), new
			{
				partition,
				node,
				key,
				timestamp,
				timezone
			});
		}

		public Guid InsertNode(string name, string connectionString, string adminConnectionString, NodeStatus status)
		{
			return Connection.Post<Guid>(Connection.CreateUrl(Controller, "InsertNode"), new
			{
				name,
				connectionString,
				adminConnectionString,
				status = status.ToString()
			});
		}

		public void InsertPartition(Guid configuration, string name, PartitionStatus status, Guid resourceGroup)
		{
			Connection.Post(Connection.CreateUrl(Controller, "InsertPartition"), new
			{
				name,
				configuration,
				status,
				resourceGroup
			});
		}

		public Guid InsertTimeZone(string name, int offset)
		{
			return Connection.Post<Guid>(Connection.CreateUrl(Controller, "InsertTimeZone"), new
			{
				name,
				offset
			});
		}

		public Guid InsertTransaction(Guid partition, int blockCount, Guid timezone)
		{
			return Connection.Post<Guid>(Connection.CreateUrl(Controller, "InsertTransaction"), new
			{
				partition,
				blockCount,
				timezone
			});
		}

		public Guid InsertTransactionBlock(Guid transaction)
		{
			return Connection.Post<Guid>(Connection.CreateUrl(Controller, "InsertTransactionBlock"), new
			{
				transaction
			});
		}

		public Guid LockFile(Guid token)
		{
			return Connection.Post<Guid>(Connection.CreateUrl(Controller, "LockFile"), new { token });
		}

		public void PingMaintenance(Guid popReceipt, int nextVisible)
		{
			Connection.Post(Connection.CreateUrl(Controller, "PingMaintenance"), new
			{
				popReceipt,
				nextVisible
			});
		}

		public void PingTransactionBlock(Guid popReceipt, int nextVisible)
		{
			Connection.Post(Connection.CreateUrl(Controller, "PingTransactionBlock"), new
			{
				popReceipt,
				nextVisible = nextVisible
			});
		}

		public ImmutableList<IPartitionBufferData> QueryBufferData(Guid partition)
		{
			return Connection.Post<List<PartitionBufferData>>(Connection.CreateUrl(Controller, "QueryBufferData"), new
			{
				partition,
			}).ToImmutableList<IPartitionBufferData>();
		}

		public ImmutableList<IPartitionFieldStatistics> QueryFieldStatistics()
		{
			return Connection.Get<List<PartitionFieldStatistics>>(Connection.CreateUrl(Controller, "QueryFieldStatistics")).ToImmutableList<IPartitionFieldStatistics>();
		}

		public ImmutableList<IPartitionFile> QueryFiles()
		{
			return Connection.Get<List<PartitionFile>>(Connection.CreateUrl(Controller, "QueryFiles")).ToImmutableList<IPartitionFile>();

		}

		public ImmutableList<IPartitionFile> QueryFilesForPartition(Guid partition)
		{
			return Connection.Post<List<PartitionFile>>(Connection.CreateUrl(Controller, "QueryFilesForPartition"), new
			{
				partition
			}).ToImmutableList<IPartitionFile>();
		}

		public ImmutableList<INode> QueryNodes()
		{
			return Connection.Get<List<Node>>(Connection.CreateUrl(Controller, "QueryNodes")).ToImmutableList<INode>();
		}

		public ImmutableList<IPartition> QueryPartitions()
		{
			return Connection.Get<List<Partition>>(Connection.CreateUrl(Controller, "QueryPartitions")).ToImmutableList<IPartition>();
		}

		public ImmutableList<ITimeZone> QueryTimeZones()
		{
			return Connection.Get<List<TimeZone>>(Connection.CreateUrl(Controller, "QueryTimeZones")).ToImmutableList<ITimeZone>();
		}

		public IPartitionFieldStatistics SelectFieldStatistic(Guid file, string fileName)
		{
			return Connection.Post<PartitionFieldStatistics>(Connection.CreateUrl(Controller, "SelectFieldStatistic"), new
			{
				file,
				fileName
			});
		}

		public IPartitionFile SelectFile(Guid token)
		{
			return Connection.Post<PartitionFile>(Connection.CreateUrl(Controller, "SelectFile"), new
			{
				token
			});
		}

		public INode SelectNode(Guid node)
		{
			return Connection.Post<Node>(Connection.CreateUrl(Controller, "SelectNode"), new
			{
				token = node
			});
		}

		public IPartition SelectPartition(Guid configuration)
		{
			return Connection.Post<Partition>(Connection.CreateUrl(Controller, "SelectPartition"), new
			{
				configuration
			});
		}

		public ITimeZone SelectTimeZone(Guid token)
		{
			return Connection.Post<TimeZone>(Connection.CreateUrl(Controller, "SelectTimeZone"), new
			{
				token
			});
		}

		public ITransactionBlock SelectTransactionBlock(Guid token)
		{
			return Connection.Post<TransactionBlock>(Connection.CreateUrl(Controller, "SelectTransactionBlock"), new
			{
				token
			});
		}

		public void UnlockFile(Guid unlockKey)
		{
			Connection.Post(Connection.CreateUrl(Controller, "UnlockFile"), new
			{
				unlockKey
			});
		}

		public void UpdateFieldStatistics(Guid file, string fieldName, string startString, string endString, double startNumber, double endNumber, DateTime startDate, DateTime endDate)
		{
			Connection.Post(Connection.CreateUrl(Controller, "UpdateFieldStatistics"), new
			{
				file,
				fieldName,
				startString,
				endString,
				startNumber,
				endNumber,
				startDate,
				endDate
			});
		}

		public void UpdateFile(Guid token, DateTime startTimestamp, DateTime endTimestamp, int count, PartitionFileStatus status)
		{
			Connection.Post(Connection.CreateUrl(Controller, "UpdateFile"), new
			{
				token,
				startTimestamp,
				endTimestamp,
				count,
				status = status.ToString()
			});
		}

		public void UpdateNode(Guid token, string name, string connectionString, string adminConnectionString, NodeStatus status, long size)
		{
			Connection.Post(Connection.CreateUrl(Controller, "UpdateNode"), new
			{
				token,
				name,
				connectionString,
				adminConnectionString,
				status = status.ToString(),
				size
			});
		}

		public void UpdatePartition(Guid configuration, string name, PartitionStatus status)
		{
			Connection.Post(Connection.CreateUrl(Controller, "UpdatePartition"), new
			{
				configuration,
				name,
				status = status.ToString()
			});
		}

		public void UpdateTimeZone(Guid token, string name, int offset)
		{
			Connection.Post(Connection.CreateUrl(Controller, "UpdateTimeZone"), new
			{
				token,
				name,
				offset
			});
		}
	}
}
