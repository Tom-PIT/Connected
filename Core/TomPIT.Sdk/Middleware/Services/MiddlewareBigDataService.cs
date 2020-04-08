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

		public void Update<T>(string partition, T item)
		{
			var descriptor = ComponentDescriptor.BigDataPartition(Context, partition);

			Context.Tenant.GetService<IBigDataService>().Update(descriptor.Configuration as IPartitionConfiguration, item);
		}
	}
}
