using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.BigData;
using TomPIT.ComponentModel.BigData;
using TomPIT.Configuration;

namespace TomPIT.Services.Context
{
	internal class ContextBigDataService : ContextClient, IContextBigDataService
	{
		public ContextBigDataService(IExecutionContext context) : base(context)
		{
		}

		public void Add<T>(string partition, T item)
		{
			Add(partition, new List<T> { item });
		}

		public void Add<T>(string partition, List<T> items)
		{
			var descriptor = ConfigurationDescriptor.Parse(Connection, partition, "BigDataPartition");

			Connection.GetService<IBigDataService>().Add(descriptor.Configuration as IPartitionConfiguration, items);
		}
	}
}
