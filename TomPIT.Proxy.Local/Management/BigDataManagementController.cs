using System;
using System.Collections.Immutable;
using TomPIT.BigData;
using TomPIT.Proxy.Management;
using TomPIT.Storage;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management
{
    internal class BigDataManagementController : IBigDataManagementController
    {
        public void ActivateTransaction(Guid transaction)
        {
            DataModel.BigDataTransactions.Activate(transaction);
        }

        public void ClearBufferData(Guid partition, TimeSpan nextVisible, long id)
        {
            DataModel.BigDataPartitionBuffering.Clear(partition, nextVisible, id);
        }

        public void CompleteMaintenance(Guid popReceipt)
        {
            DataModel.BigDataPartitions.Complete(popReceipt);
        }

        public void CompleteTransactionBlock(Guid popReceipt)
        {
            DataModel.BigDataTransactionBlocks.Complete(popReceipt);
        }

        public void DeleteFile(Guid token)
        {
            DataModel.BigDataPartitionFiles.Delete(token);
        }

        public void DeleteNode(Guid token)
        {
            DataModel.BigDataNodes.Delete(token);
        }

        public void DeletePartition(Guid configuration)
        {
            DataModel.BigDataPartitions.Delete(configuration);
        }

        public void DeleteTimeZone(Guid token)
        {
            DataModel.BigDataTimeZones.Delete(token);
        }

        public void DeleteTransaction(Guid transaction)
        {
            DataModel.BigDataTransactions.Delete(transaction);
        }

        public ImmutableList<IPartitionBuffer> DequeueBuffers(int count, TimeSpan timespan)
        {
            return DataModel.BigDataPartitionBuffering.Dequeue(count, timespan).ToImmutableList<IPartitionBuffer>();
        }

        public ImmutableList<IQueueMessage> DequeueMaintenance(int count, int nextVisible)
        {
            return DataModel.BigDataPartitions.DequeueMaintenance(count, TimeSpan.FromSeconds(nextVisible));
        }

        public ImmutableList<IQueueMessage> DequeueTransactionBlocks(int count, int nextVisible)
        {
            return DataModel.BigDataTransactionBlocks.Dequeue(count, TimeSpan.FromSeconds(nextVisible));
        }

        public void EnqueueBuffer(Guid partition, TimeSpan duration, byte[] data)
        {
            DataModel.BigDataPartitionBuffering.Enqueue(partition, duration, data);
        }

        public Guid InsertFile(Guid partition, Guid node, string key, DateTime timestamp, Guid timezone)
        {
            return DataModel.BigDataPartitionFiles.Insert(partition, node, timezone, key, timestamp);
        }

        public Guid InsertNode(string name, string connectionString, string adminConnectionString, NodeStatus status)
        {
            return DataModel.BigDataNodes.Insert(name, connectionString, adminConnectionString, status);
        }

        public void InsertPartition(Guid configuration, string name, PartitionStatus status, Guid resourceGroup)
        {
            DataModel.BigDataPartitions.Insert(configuration, name, status, resourceGroup);
        }

        public Guid InsertTimeZone(string name, int offset)
        {
            return DataModel.BigDataTimeZones.Insert(name, offset);
        }

        public Guid InsertTransaction(Guid partition, int blockCount, Guid timezone)
        {
            return DataModel.BigDataTransactions.Insert(partition, timezone, blockCount);
        }

        public Guid InsertTransactionBlock(Guid transaction)
        {
            return DataModel.BigDataTransactionBlocks.Insert(transaction);
        }

        public Guid LockFile(Guid token)
        {
            return DataModel.BigDataPartitionFiles.Lock(token);
        }

        public void PingMaintenance(Guid popReceipt, int nextVisible)
        {
            DataModel.BigDataPartitions.Ping(popReceipt, TimeSpan.FromSeconds(nextVisible));
        }

        public void PingTransactionBlock(Guid popReceipt, int nextVisible)
        {
            DataModel.BigDataTransactionBlocks.Ping(popReceipt, TimeSpan.FromSeconds(nextVisible));
        }

        public ImmutableList<IPartitionBufferData> QueryBufferData(Guid partition)
        {
            return DataModel.BigDataPartitionBuffering.QueryData(partition).ToImmutableList<IPartitionBufferData>();
        }

        public ImmutableList<IPartitionFieldStatistics> QueryFieldStatistics()
        {
            return DataModel.BigDataPartitionFieldStatistics.Query();
        }

        public ImmutableList<IPartitionFile> QueryFiles()
        {
            return DataModel.BigDataPartitionFiles.Query();
        }

        public ImmutableList<IPartitionFile> QueryFilesForPartition(Guid partition)
        {
            return DataModel.BigDataPartitionFiles.Query(partition);
        }

        public ImmutableList<INode> QueryNodes()
        {
            return DataModel.BigDataNodes.Query();
        }

        public ImmutableList<IPartition> QueryPartitions()
        {
            return DataModel.BigDataPartitions.Query();
        }

        public ImmutableList<ITimeZone> QueryTimeZones()
        {
            return DataModel.BigDataTimeZones.Query();
        }

        public IPartitionFieldStatistics SelectFieldStatistic(Guid file, string fileName)
        {
            return DataModel.BigDataPartitionFieldStatistics.Select(file, fileName);
        }

        public IPartitionFile SelectFile(Guid token)
        {
            return DataModel.BigDataPartitionFiles.Select(token);
        }

        public INode SelectNode(Guid node)
        {
            return DataModel.BigDataNodes.Select(node);
        }

        public IPartition SelectPartition(Guid configuration)
        {
            return DataModel.BigDataPartitions.Select(configuration);
        }

        public ITimeZone SelectTimeZone(Guid token)
        {
            return DataModel.BigDataTimeZones.Select(token);
        }

        public ITransactionBlock SelectTransactionBlock(Guid token)
        {
            return DataModel.BigDataTransactionBlocks.Select(token);
        }

        public void UnlockFile(Guid unlockKey)
        {
            DataModel.BigDataPartitionFiles.Unlock(unlockKey);
        }

        public void UpdateFieldStatistics(Guid file, string fieldName, string startString, string endString, double startNumber, double endNumber, DateTime startDate, DateTime endDate)
        {
            DataModel.BigDataPartitionFieldStatistics.Update(file, fieldName, startString, endString, startNumber, endNumber, startDate, endDate);
        }

        public void UpdateFile(Guid token, DateTime startTimestamp, DateTime endTimestamp, int count, PartitionFileStatus status)
        {
            DataModel.BigDataPartitionFiles.Update(token, startTimestamp, endTimestamp, count, status);
        }

        public void UpdateNode(Guid token, string name, string connectionString, string adminConnectionString, NodeStatus status, long size)
        {
            DataModel.BigDataNodes.Update(token, name, connectionString, adminConnectionString, status, size);
        }

        public void UpdatePartition(Guid configuration, string name, PartitionStatus status)
        {
            DataModel.BigDataPartitions.Update(configuration, name, status);
        }

        public void UpdateTimeZone(Guid token, string name, int offset)
        {
            DataModel.BigDataTimeZones.Update(token, name, offset);
        }
    }
}
