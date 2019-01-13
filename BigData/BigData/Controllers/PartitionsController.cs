using Amt.DataHub.Partitions;
using Amt.Sdk.Filters;
using Amt.Sys.Model.DataHub;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Amt.DataHub.Controllers
{
	[ManagementAuthentication]
	public class PartitionsController : ApiController
	{
		[HttpGet]
		public List<PartitionFile> QueryFiles(int partitionId)
		{
			return PartitionModel.QueryFiles(partitionId);
		}
		[HttpGet]
		public List<Partition> QueryPartitions()
		{
			return PartitionModel.QueryPartitions();
		}

		[HttpGet]
		public PartitionFile SelectFile(long id)
		{
			return PartitionModel.SelectFile(id);
		}
		[HttpGet]
		public Partition SelectPartitionByIdentifier(Guid identifier)
		{
			return PartitionModel.SelectPartition(identifier);
		}
		[HttpGet]
		public Partition SelectPartitionById(int id)
		{
			return PartitionModel.SelectPartition(id);
		}
		[HttpPost]
		public void SynchronizePartitions()
		{
			PartitionModel.SynchronizePartitions();
		}
		[HttpPost]
		public void UpdatePartitionStatus(int id, int status)
		{
			PartitionModel.UpdatePartitionStatus(id, status);
		}
	}
}