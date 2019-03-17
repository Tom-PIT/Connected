using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Management
{
	public class BigDataManagementController : SysController
	{
		/*
		 * Nodes
		 */
		[HttpGet]
		public List<INode> QueryNodes()
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
			var connectionString = body.Required<string>("connectionString");
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
		 * Partitions
		 */
		[HttpGet]
		public List<IPartition> QueryPartitions()
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

			DataModel.BigDataPartitions.Insert(configuration, name, status);
		}

		[HttpPost]
		public void UpdatePartition()
		{
			var body = FromBody();
			var configuration = body.Required<Guid>("configuration");
			var name = body.Required<string>("name");
			var status = body.Required<PartitionStatus>("status");
			var fileCount = body.Required<int>("fileCount");

			DataModel.BigDataPartitions.Update(configuration, name, status, fileCount);
		}

		[HttpPost]
		public void DeletePartition()
		{
			var body = FromBody();
			var configuration = body.Required<Guid>("configuration");

			DataModel.BigDataPartitions.Delete(configuration);
		}
	}
}
