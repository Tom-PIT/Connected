using System;
using System.Collections.Immutable;
using TomPIT.BigData;
using TomPIT.Storage;

namespace TomPIT.Proxy.Management
{
	public interface IBigDataManagementController
	{
		ImmutableList<INode> QueryNodes();
		INode SelectNode(Guid node);
		Guid InsertNode(string name, string connectionString, string adminConnectionString, NodeStatus status);
		void UpdateNode(Guid token, string name, string connectionString, string adminConnectionString, NodeStatus status, long size);
		void DeleteNode(Guid token);
		ImmutableList<ITimeZone> QueryTimeZones();
		ITimeZone SelectTimeZone(Guid token);
		Guid InsertTimeZone(string name, int offset);
		void UpdateTimeZone(Guid token, string name, int offset);
		void DeleteTimeZone(Guid token);
		ImmutableList<IPartition> QueryPartitions();
		IPartition SelectPartition(Guid configuration);
		void InsertPartition(Guid configuration, string name, PartitionStatus status, Guid resourceGroup);
		void UpdatePartition(Guid configuration, string name, PartitionStatus status);
		void DeletePartition(Guid configuration);
		void ActivateTransaction(Guid transaction);
		Guid InsertTransaction(Guid partition, int blockCount, Guid timezone);
		void DeleteTransaction(Guid transaction);
		Guid InsertTransactionBlock(Guid transaction);
		void CompleteTransactionBlock(Guid popReceipt);
		ImmutableList<IQueueMessage> DequeueTransactionBlocks(int count, int nextVisible);
		void PingTransactionBlock(Guid popReceipt, int nextVisible);
		ITransactionBlock SelectTransactionBlock(Guid token);
		Guid InsertFile(Guid partition, Guid node, string key, DateTime timestamp, Guid timezone);
		void UpdateFile(Guid token, DateTime startTimestamp, DateTime endTimestamp, int count, PartitionFileStatus status);
		ImmutableList<IPartitionFile> QueryFiles();
		ImmutableList<IPartitionFile> QueryFilesForPartition(Guid partition);
		IPartitionFile SelectFile(Guid token);
		Guid LockFile(Guid token);
		void UnlockFile(Guid unlockKey);
		void DeleteFile(Guid token);
		ImmutableList<IPartitionFieldStatistics> QueryFieldStatistics();
		IPartitionFieldStatistics SelectFieldStatistic(Guid file, string fileName);
		void UpdateFieldStatistics(Guid file, string fieldName, string startString, string endString, double startNumber, double endNumber, DateTime startDate, DateTime endDate);
		ImmutableList<IQueueMessage> DequeueMaintenance(int count, int nextVisible);
		void PingMaintenance(Guid popReceipt, int nextVisible);
		void CompleteMaintenance(Guid popReceipt);
		ImmutableList<IPartitionBuffer> DequeueBuffers(int count, TimeSpan timespan);
		void EnqueueBuffer(Guid partition, TimeSpan duration, byte[] data);
		ImmutableList<IPartitionBufferData> QueryBufferData(Guid partition);
		void ClearBufferData(Guid partition, TimeSpan nextVisible, long id);
	}
}
