using TomPIT.Connectivity;

namespace TomPIT.Analytics
{
	internal class AnalyticsService : TenantObject, IAnalyticsService
	{
		private IMruService _mru = null;

		public AnalyticsService(ITenant tenant) : base(tenant)
		{

		}

		public IMruService Mru
		{
			get
			{
				if (_mru == null)
					_mru = new MruService(Tenant);

				return _mru;
			}
		}
	}
}
