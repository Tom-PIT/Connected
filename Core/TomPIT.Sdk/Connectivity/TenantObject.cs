namespace TomPIT.Connectivity
{
	public abstract class TenantObject : ITenantObject
	{
		protected TenantObject()
		{
		}

		protected TenantObject(ITenant tenant)
		{
			Tenant = tenant;
		}

		public ITenant Tenant { get; protected set; }
	}
}