using System;

namespace TomPIT.Connectivity
{
	public class TenantArgs : EventArgs
	{
		public TenantArgs(ITenant tenant)
		{
			Tenant = tenant;
		}

		public ITenant Tenant { get; }
	}
}
