using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	internal class CdnService : TenantObject, ICdnService
	{
		public CdnService(ITenant tenant) : base(tenant)
		{
		}

		public ICdnEventConnection Connect(IMiddlewareContext context)
		{
			var result = new CdnEventConnection(context);

			result.Connect();

			return result;
		}
	}
}
