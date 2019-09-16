using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareBigDataService : MiddlewareObject, IMiddlewareBigDataService
	{
		public MiddlewareBigDataService(IMiddlewareContext context) : base(context)
		{
		}

		public void Add<T>(string partition, T item)
		{
			Add(partition, new List<T> { item });
		}

		public void Add<T>(string partition, List<T> items)
		{
			var descriptor = ComponentDescriptor.BigDataPartition(Context, partition);

			Context.Tenant.GetService<IBigDataService>().Add(descriptor.Configuration as IPartitionConfiguration, items);
		}
	}
}
