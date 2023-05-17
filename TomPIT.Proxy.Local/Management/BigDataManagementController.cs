using System;
using System.Collections.Immutable;
using TomPIT.BigData;
using TomPIT.Proxy.Management;
using TomPIT.Storage;

namespace TomPIT.Proxy.Local.Management
{
	internal class BigDataManagementController : IBigDataManagementController
	{
		public void ActivateTransaction(Guid transaction)
		{
			throw new NotImplementedException();
		}

		public void ClearBufferData(Guid partition, TimeSpan nextVisible, long id)
		{
			throw new NotImplementedException();
		}

		public void CompleteMaintenance(Guid popReceipt)
		{
			throw new NotImplementedException();
		}

		public void CompleteTransactionBlock(Guid popReceipt)
		{
			throw new NotImplementedException();
		}

		public void DeleteFile(Guid token)
		{
			throw new NotImplementedException();
		}

		public void DeleteNode(Guid token)
		{
			throw new NotImplementedException();
		}

		public void DeletePartition(Guid configuration)
		{
			throw new NotImplementedException();
		}

		public void DeleteTimeZone(Guid token)
		{
			throw new NotImplementedException();
		}

		public void DeleteTransaction(Guid transaction)
		{
			throw new NotImplementedException();
		}

		public ImmutableList<IPartitionBuffer> DequeueBuffers(int count, TimeSpan timespan)
		{
			throw new NotImplementedException();
		}

		public ImmutableList<IQueueMessage> DequeueMaintenance(int count, int nextVisible)
		{
			throw new NotImplementedException();
		}

		public ImmutableList<IQueueMessage> DequeueTransactionBlocks(int count, int nextVisible)
		{
			throw new NotImplementedException();
		}

		public void EnqueueBuffer(Guid partition, TimeSpan duration, byte[] data)
		{
			throw new NotImplementedException();
		}

		public Guid InsertFile(Guid partition, Guid node, string key, DateTime timestamp, Guid timezone)
		{
			throw new NotImplementedException();
		}

		public Guid InsertNode(string name, string connectionString, string adminConnectionString, NodeStatus status)
		{
			throw new NotImplementedException();
		}

		public void InsertPartition(Guid configuration, string name, PartitionStatus status, Guid resourceGroup)
		{
			throw new NotImplementedException();
		}

		public Guid InsertTimeZone(string name, int offset)
		{
			throw new NotImplementedException();
		}

		public Guid InsertTransaction(Guid partition, int blockCount, Guid timezone)
		{
			throw new NotImplementedException();
		}

		public Guid InsertTransactionBlock(Guid transaction)
		{
			throw new NotImplementedException();
		}

		public Guid LockFile(Guid token)
		{
			throw new NotImplementedException();
		}

		public void PingMaintenance(Guid popReceipt, int nextVisible)
		{
			throw new NotImplementedException();
		}

		public void PingTransactionBlock(Guid popReceipt, int nextVisible)
		{
			throw new NotImplementedException();
		}

		public ImmutableList<IPartitionBufferData> QueryBufferData(Guid partition)
		{
			throw new NotImplementedException();
		}

		public ImmutableList<IPartitionFieldStatistics> QueryFieldStatistics()
		{
			throw new NotImplementedException();
		}

		public ImmutableList<IPartitionFile> QueryFiles()
		{
			throw new NotImplementedException();
		}

		public ImmutableList<IPartitionFile> QueryFilesForPartition(Guid partition)
		{
			throw new NotImplementedException();
		}

		public ImmutableList<INode> QueryNodes()
		{
			throw new NotImplementedException();
		}

		public ImmutableList<IPartition> QueryPartitions()
		{
			throw new NotImplementedException();
		}

		public ImmutableList<ITimeZone> QueryTimeZones()
		{
			throw new NotImplementedException();
		}

		public IPartitionFieldStatistics SelectFieldStatistic(Guid file, string fileName)
		{
			throw new NotImplementedException();
		}

		public IPartitionFile SelectFile(Guid token)
		{
			throw new NotImplementedException();
		}

		public INode SelectNode(Guid node)
		{
			throw new NotImplementedException();
		}

		public IPartition SelectPartition(Guid configuration)
		{
			throw new NotImplementedException();
		}

		public ITimeZone SelectTimeZone(Guid token)
		{
			throw new NotImplementedException();
		}

		public ITransactionBlock SelectTransactionBlock(Guid token)
		{
			throw new NotImplementedException();
		}

		public void UnlockFile(Guid unlockKey)
		{
			throw new NotImplementedException();
		}

		public void UpdateFieldStatistics(Guid file, string fieldName, string startString, string endString, double startNumber, double endNumber, DateTime startDate, DateTime endDate)
		{
			throw new NotImplementedException();
		}

		public void UpdateFile(Guid token, DateTime startTimestamp, DateTime endTimestamp, int count, PartitionFileStatus status)
		{
			throw new NotImplementedException();
		}

		public void UpdateNode(Guid token, string name, string connectionString, string adminConnectionString, NodeStatus status, long size)
		{
			throw new NotImplementedException();
		}

		public void UpdatePartition(Guid configuration, string name, PartitionStatus status)
		{
			throw new NotImplementedException();
		}

		public void UpdateTimeZone(Guid token, string name, int offset)
		{
			throw new NotImplementedException();
		}
	}
}
